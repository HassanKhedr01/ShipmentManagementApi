using System.Reflection;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ShipmentManagement.Api.Filters;

public class GlobalJsonConsumesFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            await next();
            return;
        }

        var hasConsumesAttribute =
            actionDescriptor.MethodInfo.GetCustomAttributes<ConsumesAttribute>().Any()
            || actionDescriptor.ControllerTypeInfo.GetCustomAttributes<ConsumesAttribute>().Any();

        if (hasConsumesAttribute)
        {
            await next();
            return;
        }

        var hasBodyParameter = actionDescriptor.Parameters
            .OfType<ControllerParameterDescriptor>()
            .Any(parameter =>
                parameter.ParameterInfo.GetCustomAttribute<FromBodyAttribute>() != null
                || parameter.BindingInfo?.BindingSource == BindingSource.Body);

        if (!hasBodyParameter)
        {
            await next();
            return;
        }

        var contentType = context.HttpContext.Request.ContentType;

        if (contentType == null || !contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new UnsupportedMediaTypeResult();
            return;
        }

        await next();
    }
}
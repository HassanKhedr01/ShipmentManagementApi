using System.Text.Json.Serialization;

using Asp.Versioning;
using Asp.Versioning.Conventions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Serilog;

using ShipmentManagement.Api.Configurations;
using ShipmentManagement.Api.Exceptions;
using ShipmentManagement.Api.Filters;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace ShipmentManagement.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddSerilogLogging(this IServiceCollection services, WebApplicationBuilder builder)
    {
        services.AddSerilog((serviceProvider, configuration) =>
        {
            configuration.ReadFrom.Services(serviceProvider).Enrich.FromLogContext();

            if (builder.Environment.IsEnvironment("Testing"))
            {
                configuration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                    .WriteTo.Console(
                        outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] " +
                        "{Message:lj} [CorrelationId: {CorrelationId}]" +
                        "{NewLine}{Exception}");

                return;
            }

            configuration.ReadFrom.Configuration(builder.Configuration);
        });

        return services;
    }


    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }

    public static IServiceCollection AddControllersWithFilters(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<LogActionsFilter>();
            options.Filters.Add<GlobalJsonConsumesFilter>();
            options.Filters.Add(new ProducesAttribute("application/json"));
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        return services;
    }

    public static IServiceCollection AddApiVersioningWithExplorer(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            }).AddMvc(options => { options.Conventions.Add(new VersionByNamespaceConvention()); })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
        return services;
    }

    public static IServiceCollection AddSwaggerGenOptions(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        return services;
    }

    public static IServiceCollection ConfigureApiBehaviorOptions(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });
        return services;
    }

    public static IServiceCollection AddTimeProvider(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
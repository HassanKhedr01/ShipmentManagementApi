using Serilog;
using Asp.Versioning.ApiExplorer;
using ShipmentManagement.Application;
using ShipmentManagement.Api.Middleware;
using ShipmentManagement.Infrastructure;

namespace ShipmentManagement.Api;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console().CreateBootstrapLogger();
        try
        {
            Log.Information("Starting application..");

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSerilogLogging(builder);
            builder.Services.AddGlobalExceptionHandler();
            builder.Services.AddProblemDetails();
            builder.Services.AddControllersWithFilters();
            builder.Services.AddApiVersioningWithExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGenOptions();
            builder.Services.ConfigureApiBehaviorOptions();
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddAuthorization();
            builder.Services.AddTimeProvider();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler();
            }

            app.UseCorrelationId();
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName.ToUpperInvariant());
                    }

                    options.DocumentTitle = "Shipment Management and Tracking API";
                    options.DisplayRequestDuration();
                    options.EnableDeepLinking();
                    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        catch (HostAbortedException) { }
        catch (Exception e)
        {
            Log.Fatal(e, "Application terminated unexpectedly.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
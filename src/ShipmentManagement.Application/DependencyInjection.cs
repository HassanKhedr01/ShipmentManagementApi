using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

using ShipmentManagement.Application.Interfaces;
using ShipmentManagement.Application.Services;
using ShipmentManagement.Application.Validators.Packages;

namespace ShipmentManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreatePackageV1DtoValidator>();

        services.AddScoped<IPackagesService, PackagesService>();
        services.AddScoped<IShipmentService, ShipmentService>();
        services.AddScoped<IFacilitiesService, FacilitiesService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        
        return services;
    }
}
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShipmentManagement.Application.Interfaces;
using ShipmentManagement.Infrastructure.Identity;
using ShipmentManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using ShipmentManagement.Domain.Repositories;
using ShipmentManagement.Infrastructure.Persistence;
using ShipmentManagement.Infrastructure.Repositories;

namespace ShipmentManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySQL(configuration.GetConnectionString("DefaultConnection")!));

        services.AddDataProtection();
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddHttpContextAccessor();
        services.AddScoped<IPackagesRepository, PackagesRepository>();
        services.AddScoped<ITrackingRepository, TrackingRepository>();
        services.AddScoped<IFacilitiesRepository, FacilitiesRepository>();
        services.AddScoped<IAttemptsRepository, AttemptsRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        return services;
    }
}
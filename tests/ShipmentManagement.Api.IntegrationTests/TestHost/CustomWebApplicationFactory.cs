using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using ShipmentManagement.Api.IntegrationTests.TestData;
using ShipmentManagement.Infrastructure.Persistence;

namespace ShipmentManagement.Api.IntegrationTests.TestHost;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<SqliteConnection>();

            _connection.Open();
            services.AddSingleton(_connection);
            services.AddDbContext<AppDbContext>((container, options) =>
            {
                options.UseSqlite(container.GetRequiredService<SqliteConnection>());
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var context = host.Services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        DatabaseSeeder.SeedIdentityAsync(scope.ServiceProvider).GetAwaiter().GetResult();
        DatabaseSeeder.SeedApplicationAsync(context).GetAwaiter().GetResult();
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
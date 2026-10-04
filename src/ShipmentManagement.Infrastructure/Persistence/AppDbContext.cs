using System.Reflection;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Infrastructure.Identity;

namespace ShipmentManagement.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public DbSet<Package> Packages { get; set; }
    public DbSet<Facility> Facilities { get; set; }
    public DbSet<TrackingEvent> TrackingEvents { get; set; }
    public DbSet<DeliveryAttempt> DeliveryAttempts { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasMany(u => u.Packages)
            .WithOne()
            .HasForeignKey(p => p.ApplicationUserId);
        
        builder.Entity<ApplicationUser>()
            .HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.ApplicationUserId);
        
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
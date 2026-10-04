using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ShipmentManagement.Domain.Entities;

namespace ShipmentManagement.Infrastructure.Persistence.Config;

public class FacilityConfigurations : IEntityTypeConfiguration<Facility>
{
    public void Configure(EntityTypeBuilder<Facility> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Name).IsRequired().HasColumnType("VARCHAR").HasMaxLength(100);
        builder.Property(f => f.Address).IsRequired().HasColumnType("VARCHAR").HasMaxLength(200);
        builder.Property(f => f.City).IsRequired().HasColumnType("VARCHAR").HasMaxLength(100);
        builder.Property(f => f.IsActive).IsRequired();
        
        builder.HasMany(f => f.TrackingEvents).WithOne(te => te.Facility)
            .HasForeignKey(te => te.FacilityId).OnDelete(DeleteBehavior.Restrict);
    }
}
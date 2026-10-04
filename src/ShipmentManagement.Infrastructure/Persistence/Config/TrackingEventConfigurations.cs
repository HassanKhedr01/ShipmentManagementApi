using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ShipmentManagement.Domain.Entities;

namespace ShipmentManagement.Infrastructure.Persistence.Config;

public class TrackingEventConfigurations : IEntityTypeConfiguration<TrackingEvent>
{
    public void Configure(EntityTypeBuilder<TrackingEvent> builder)
    {
        builder.HasKey(te => te.Id);
        builder.Property(te => te.PackageName).HasColumnType("VARCHAR").HasMaxLength(100);
        builder.Property(te => te.FacilityName).HasColumnType("VARCHAR").HasMaxLength(100);
        builder.Property(te => te.Status).HasConversion<string>();
        builder.Property(te => te.OccuredAt).HasColumnType("DATETIME");
        builder.Property(te => te.Description).HasColumnType("VARCHAR").HasMaxLength(200);
    }
}
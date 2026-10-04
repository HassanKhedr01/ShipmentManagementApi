using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ShipmentManagement.Domain.Entities;

namespace ShipmentManagement.Infrastructure.Persistence.Config;

public class DeliveryAttemptConfigurations : IEntityTypeConfiguration<DeliveryAttempt>
{
    public void Configure(EntityTypeBuilder<DeliveryAttempt> builder)
    {
        builder.HasKey(da => da.Id);
        builder.Property(da => da.AttemptedAt).HasColumnType("DATETIME");
        builder.Property(da => da.DeliveryResult).HasConversion<string>();
        builder.Property(da => da.FailureReason).HasColumnType("VARCHAR").HasMaxLength(200);
    }
}
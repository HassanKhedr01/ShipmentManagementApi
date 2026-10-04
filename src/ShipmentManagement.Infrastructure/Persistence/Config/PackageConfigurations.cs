using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ShipmentManagement.Domain.Entities;

namespace ShipmentManagement.Infrastructure.Persistence.Config;

public class PackageConfigurations : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.TrackingNumber).IsRequired().HasColumnType("VARCHAR").HasMaxLength(50);
        builder.Property(p => p.Name).IsRequired().HasColumnType("VARCHAR").HasMaxLength(50);
        builder.Property(p => p.SenderName).IsRequired().HasColumnType("VARCHAR").HasMaxLength(100);
        builder.Property(p => p.RecipientName).IsRequired().HasColumnType("VARCHAR").HasMaxLength(100);
        builder.OwnsOne(p => p.OriginAddress, oa =>
        {
            oa.Property(a => a.Street).IsRequired().HasColumnType("VARCHAR").HasMaxLength(200);
            oa.Property(a => a.City).IsRequired().HasColumnType("VARCHAR").HasMaxLength(100);
            oa.Property(a => a.PostalCode).IsRequired().HasColumnType("VARCHAR").HasMaxLength(20);
        });
        builder.OwnsOne(p => p.DestinationAddress, oa =>
        {
            oa.Property(a => a.Street).IsRequired().HasColumnType("VARCHAR").HasMaxLength(200);
            oa.Property(a => a.City).IsRequired().HasColumnType("VARCHAR").HasMaxLength(100);
            oa.Property(a => a.PostalCode).IsRequired().HasColumnType("VARCHAR").HasMaxLength(20);
        });
        builder.Property(p => p.CurrentStatus).HasConversion<string>();
        builder.Property(p => p.DeliveryType).HasConversion<string>();
        builder.Property(p => p.EstimatedDeliveryDate).HasColumnType("DATETIME");
        builder.Property(p => p.CreatedAt).HasColumnType("TIMESTAMP")
            .HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();

        builder.HasIndex(p => p.TrackingNumber).IsUnique();

        builder.HasMany(p => p.TrackingEvents).WithOne(te => te.Package)
            .HasForeignKey(te => te.PackageId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.DeliveryAttempts).WithOne(da => da.Package)
            .HasForeignKey(da => da.PackageId);
    }
}
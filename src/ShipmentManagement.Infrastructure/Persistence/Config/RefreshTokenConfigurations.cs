using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ShipmentManagement.Domain.Entities;

namespace ShipmentManagement.Infrastructure.Persistence.Config;

public class RefreshTokenConfigurations : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).IsRequired().HasColumnType("VARCHAR").HasMaxLength(256);
        builder.Property(x => x.ExpiresAt).IsRequired().HasColumnType("DATETIME");
        builder.Property(x => x.RevokedAt).HasColumnType("DATETIME");
        builder.Property(p => p.CreatedAt).IsRequired().HasColumnType("TIMESTAMP")
            .HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();    
    }
}
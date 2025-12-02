using CMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the SiteConfiguration entity.
/// </summary>
public sealed class SiteConfigurationConfiguration : IEntityTypeConfiguration<SiteConfiguration>
{
    public void Configure(EntityTypeBuilder<SiteConfiguration> builder)
    {
        builder.ToTable("site_configurations");

        builder.HasKey(sc => sc.Id);

        builder.Property(sc => sc.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(sc => sc.Key)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(sc => sc.Key)
            .IsUnique();

        builder.Property(sc => sc.Value)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(sc => sc.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Audit fields
        builder.Property(sc => sc.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(sc => sc.UpdatedByUser)
            .WithMany()
            .HasForeignKey(sc => sc.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(sc => sc.Category);
        builder.HasIndex(sc => new { sc.Category, sc.Key });
    }
}
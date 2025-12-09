using CMS.Domain.Entities;
using CMS.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the CustomizationSettings entity.
/// Stores complex value objects (Theme, Typography, Layout) as JSONB for flexibility.
/// </summary>
public sealed class CustomizationSettingsConfiguration : IEntityTypeConfiguration<CustomizationSettings>
{
    public void Configure(EntityTypeBuilder<CustomizationSettings> builder)
    {
        builder.ToTable("customization_settings");

        builder.HasKey(cs => cs.Id);

        builder.Property(cs => cs.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(cs => cs.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(cs => cs.Version)
            .HasColumnName("version")
            .IsRequired()
            .HasDefaultValue(1)
            .IsConcurrencyToken(); // Optimistic concurrency control

        // Store complex value objects as JSONB with custom converters
        builder.Property(cs => cs.ThemeConfiguration)
            .HasColumnType("jsonb")
            .HasColumnName("theme_configuration")
            .HasConversion(ValueObjectJsonConverters.ThemeSettingsConverter)
            .IsRequired();

        builder.Property(cs => cs.TypographyConfiguration)
            .HasColumnType("jsonb")
            .HasColumnName("typography_configuration")
            .HasConversion(ValueObjectJsonConverters.TypographySettingsConverter)
            .IsRequired();

        builder.Property(cs => cs.LayoutConfiguration)
            .HasColumnType("jsonb")
            .HasColumnName("layout_configuration")
            .HasConversion(ValueObjectJsonConverters.LayoutSettingsConverter)
            .IsRequired();

        builder.Property(cs => cs.BrandingConfiguration)
            .HasColumnType("jsonb")
            .HasColumnName("branding_configuration")
            .HasConversion(ValueObjectJsonConverters.BrandingSettingsConverter)
            .IsRequired();

        // Audit fields
        builder.Property(cs => cs.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(cs => cs.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(cs => cs.LastModifiedAt)
            .HasColumnName("last_modified_at");

        builder.Property(cs => cs.LastModifiedBy)
            .HasColumnName("last_modified_by");

        // Relationships
        builder.HasOne(cs => cs.UpdatedByUser)
            .WithMany()
            .HasForeignKey(cs => cs.LastModifiedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(cs => cs.IsActive)
            .HasDatabaseName("ix_customization_settings_is_active");

        builder.HasIndex(cs => cs.LastModifiedAt)
            .HasDatabaseName("ix_customization_settings_last_modified_at");

        // Unique constraint: Only one active configuration at a time
        builder.HasIndex(cs => cs.IsActive)
            .IsUnique()
            .HasFilter("is_active = true")
            .HasDatabaseName("ix_customization_settings_unique_active");
    }
}

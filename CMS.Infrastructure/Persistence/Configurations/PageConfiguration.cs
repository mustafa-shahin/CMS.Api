using CMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Page entity.
/// </summary>
public sealed class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.ToTable("pages");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(p => p.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.Slug)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Components)
            .HasColumnType("jsonb");

        builder.Property(p => p.MetaTitle)
            .HasMaxLength(255);

        builder.Property(p => p.MetaDescription)
            .HasMaxLength(500);

        builder.Property(p => p.Version)
            .HasDefaultValue(1);

        // Audit fields
        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(p => p.CreatedByUser)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.UpdatedByUser)
            .WithMany()
            .HasForeignKey(p => p.UpdatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Versions)
            .WithOne(v => v.Page)
            .HasForeignKey(v => v.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedByUserId);
        builder.HasIndex(p => p.PublishedAt);
        builder.HasIndex(p => new { p.Status, p.Slug });
    }
}
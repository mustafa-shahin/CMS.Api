using CMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the PageVersion entity.
/// </summary>
public sealed class PageVersionConfiguration : IEntityTypeConfiguration<PageVersion>
{
    public void Configure(EntityTypeBuilder<PageVersion> builder)
    {
        builder.ToTable("page_versions");

        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(pv => pv.Version)
            .IsRequired();

        builder.Property(pv => pv.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(pv => pv.Components)
            .HasColumnType("jsonb");

        builder.Property(pv => pv.ChangeNotes)
            .HasMaxLength(1000);

        builder.Property(pv => pv.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(pv => pv.Page)
            .WithMany(p => p.Versions)
            .HasForeignKey(pv => pv.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pv => pv.CreatedByUser)
            .WithMany()
            .HasForeignKey(pv => pv.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(pv => pv.PageId);
        builder.HasIndex(pv => pv.CreatedByUserId);
        builder.HasIndex(pv => new { pv.PageId, pv.Version })
            .IsUnique();
    }
}
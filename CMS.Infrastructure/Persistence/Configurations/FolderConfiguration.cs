using CMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Folder entity.
/// </summary>
public sealed class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.ToTable("folders");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(f => f.Name)
            .HasMaxLength(255)
            .IsRequired();

        // Audit fields
        builder.Property(f => f.CreatedAt)
            .IsRequired();

        // Self-referencing relationship for folder hierarchy
        builder.HasOne(f => f.Parent)
            .WithMany(f => f.Children)
            .HasForeignKey(f => f.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.CreatedByUser)
            .WithMany()
            .HasForeignKey(f => f.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(f => f.ParentId);
        builder.HasIndex(f => f.CreatedByUserId);
        builder.HasIndex(f => new { f.ParentId, f.Name })
            .IsUnique();
    }
}
using CMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for ImageEntity.
/// </summary>
public sealed class ImageEntityConfiguration : IEntityTypeConfiguration<ImageEntity>
{
    public void Configure(EntityTypeBuilder<ImageEntity> builder)
    {
        builder.ToTable("Images");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.OriginalName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(i => i.ContentType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Size)
            .IsRequired();

        builder.Property(i => i.Data)
            .IsRequired()
            .HasColumnType("bytea"); // PostgreSQL byte array type

        builder.Property(i => i.StoragePath)
            .HasMaxLength(1000); // Now optional

        builder.Property(i => i.PublicUrl)
            .HasMaxLength(2000);

        builder.Property(i => i.Width)
            .IsRequired();

        builder.Property(i => i.Height)
            .IsRequired();

        builder.Property(i => i.AltText)
            .HasMaxLength(500);

        builder.Property(i => i.Caption)
            .HasMaxLength(1000);

        builder.Property(i => i.ThumbnailData)
            .HasColumnType("bytea"); // PostgreSQL byte array type for thumbnail

        builder.Property(i => i.ThumbnailPath)
            .HasMaxLength(1000); // Optional

        builder.Property(i => i.ThumbnailUrl)
            .HasMaxLength(2000);

        builder.Property(i => i.MediumData)
            .HasColumnType("bytea"); // PostgreSQL byte array type for medium version

        builder.Property(i => i.MediumPath)
            .HasMaxLength(1000); // Optional

        builder.Property(i => i.MediumUrl)
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(i => i.Folder)
            .WithMany()
            .HasForeignKey(i => i.FolderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.UploadedByUser)
            .WithMany()
            .HasForeignKey(i => i.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(i => i.FileName);
        builder.HasIndex(i => i.FolderId);
        builder.HasIndex(i => i.UploadedByUserId);
        builder.HasIndex(i => i.CreatedAt);
        builder.HasIndex(i => i.ContentType);

        // Composite index for common query patterns
        builder.HasIndex(i => new { i.UploadedByUserId, i.CreatedAt });
    }
}

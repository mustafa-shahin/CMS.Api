using CMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CMS.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the FileEntity.
/// </summary>
public sealed class FileEntityConfiguration : IEntityTypeConfiguration<FileEntity>
{
    public void Configure(EntityTypeBuilder<FileEntity> builder)
    {
        builder.ToTable("files");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(f => f.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(f => f.OriginalName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(f => f.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.Size)
            .IsRequired();

        builder.Property(f => f.StoragePath)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(f => f.PublicUrl)
            .HasMaxLength(1000);

        // Audit fields
        builder.Property(f => f.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(f => f.Folder)
            .WithMany(folder => folder.Files)
            .HasForeignKey(f => f.FolderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(f => f.UploadedByUser)
            .WithMany()
            .HasForeignKey(f => f.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(f => f.FileName)
            .IsUnique();

        builder.HasIndex(f => f.FolderId);
        builder.HasIndex(f => f.UploadedByUserId);
        builder.HasIndex(f => f.ContentType);
        builder.HasIndex(f => f.CreatedAt);
    }
}
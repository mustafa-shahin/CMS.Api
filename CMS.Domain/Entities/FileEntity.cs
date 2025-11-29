using CMS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace CMS.Domain.Entities
{
    public sealed class FileEntity : BaseAuditableEntity
    {
        public Guid Id { get; private set; }
        public string FileName { get; private set; } = null!;
        public string OriginalName { get; private set; } = null!;
        public string ContentType { get; private set; } = null!;
        public long Size { get; private set; }
        public string StoragePath { get; private set; } = null!;
        public string? PublicUrl { get; private set; }

        // Navigation properties
        public Guid? FolderId { get; private set; }
        public Folder? Folder { get; private set; }
        public Guid UploadedByUserId { get; private set; }
        public User UploadedByUser { get; private set; } = null!;

        public static FileEntity Create(string fileName, string originalName, string contentType, long size, string storagePath, Guid uploadedByUserId, Guid? folderId = null)
        {
            return new FileEntity
            {
                Id = Guid.CreateVersion7(),
                FileName = fileName,
                OriginalName = originalName,
                ContentType = contentType,
                Size = size,
                StoragePath = storagePath,
                UploadedByUserId = uploadedByUserId,
                FolderId = folderId
            };
        }

        public void MoveToFolder(Guid? folderId) { FolderId = folderId; }
        public void SetPublicUrl(string url) { PublicUrl = url; }
    }
}

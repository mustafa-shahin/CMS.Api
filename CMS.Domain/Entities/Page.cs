using CMS.Domain.Common;
using CMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CMS.Domain.Entities
{
    public sealed class Page : BaseAuditableEntity
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; } = null!;
        public string Slug { get; private set; } = null!;
        public PageStatus Status { get; private set; }
        public JsonDocument? Components { get; private set; }
        public string? MetaTitle { get; private set; }
        public string? MetaDescription { get; private set; }
        public DateTime? PublishedAt { get; private set; }
        public int Version { get; private set; }

        // Navigation properties
        public Guid CreatedByUserId { get; private set; }
        public User CreatedByUser { get; private set; } = null!;
        public Guid? UpdatedByUserId { get; private set; }
        public User? UpdatedByUser { get; private set; }
        public ICollection<PageVersion> Versions { get; private set; } = new List<PageVersion>();

        // Factory method
        public static Page Create(string title, string slug, Guid createdByUserId)
        {
            return new Page
            {
                Id = Guid.CreateVersion7(),
                Title = title,
                Slug = slug.ToLowerInvariant(),
                Status = PageStatus.Draft,
                Version = 1,
                CreatedByUserId = createdByUserId
            };
        }

        // Domain methods
        public void Update(string title, string slug, JsonDocument? components, string? metaTitle, string? metaDescription, Guid updatedByUserId) { /* ... */ }
        public void Publish(Guid publishedByUserId) { Status = PageStatus.Published; PublishedAt = DateTime.UtcNow; }
        public void Unpublish() { Status = PageStatus.Draft; PublishedAt = null; }
        public PageVersion CreateVersionSnapshot(Guid userId) { /* Create snapshot before update */ }
    }


}

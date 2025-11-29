using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CMS.Domain.Entities
{
    public sealed class PageVersion
    {
        public Guid Id { get; private set; }
        public Guid PageId { get; private set; }
        public int Version { get; private set; }
        public string Title { get; private set; } = null!;
        public JsonDocument? Components { get; private set; }
        public string? ChangeNotes { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        // Navigation
        public Page Page { get; private set; } = null!;
        public User CreatedByUser { get; private set; } = null!;

        public static PageVersion Create(Page page, Guid userId, string? changeNotes = null)
        {
            return new PageVersion
            {
                Id = Guid.CreateVersion7(),
                PageId = page.Id,
                Version = page.Version,
                Title = page.Title,
                Components = page.Components,
                ChangeNotes = changeNotes,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}

using CMS.Domain.Common;


namespace CMS.Domain.Entities
{
    public sealed class Folder : BaseAuditableEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public Guid? ParentId { get; private set; }

        // Navigation properties
        public Folder? Parent { get; private set; }
        public ICollection<Folder> Children { get; private set; } = new List<Folder>();
        public ICollection<FileEntity> Files { get; private set; } = new List<FileEntity>();
        public Guid CreatedByUserId { get; private set; }
        public User CreatedByUser { get; private set; } = null!;

        public static Folder Create(string name, Guid createdByUserId, Guid? parentId = null)
        {
            return new Folder
            {
                Id = Guid.CreateVersion7(),
                Name = name,
                ParentId = parentId,
                CreatedByUserId = createdByUserId
            };
        }

        public void Rename(string newName) { Name = newName; }
    }
}

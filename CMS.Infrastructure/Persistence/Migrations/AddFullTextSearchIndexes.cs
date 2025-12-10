using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Infrastructure.Persistence.Migrations;

/// <summary>
/// Migration to add PostgreSQL full-text search indexes for Users, Pages, and Images
/// </summary>
public partial class AddFullTextSearchIndexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add full-text search index for Users table
        // Searches across Email (weight A), FirstName (weight B), and LastName (weight B)
        migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS idx_users_search_vector
            ON ""Users"" USING GIN (
                (
                    setweight(to_tsvector('english', coalesce(""Email"", '')), 'A') ||
                    setweight(to_tsvector('english', coalesce(""FirstName"", '')), 'B') ||
                    setweight(to_tsvector('english', coalesce(""LastName"", '')), 'B')
                )
            );
        ");

        // Add full-text search index for Pages table
        // Searches across Title (weight A), Slug (weight B), MetaTitle (weight C), and MetaDescription (weight D)
        migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS idx_pages_search_vector
            ON ""Pages"" USING GIN (
                (
                    setweight(to_tsvector('english', coalesce(""Title"", '')), 'A') ||
                    setweight(to_tsvector('english', coalesce(""Slug"", '')), 'B') ||
                    setweight(to_tsvector('english', coalesce(""MetaTitle"", '')), 'C') ||
                    setweight(to_tsvector('english', coalesce(""MetaDescription"", '')), 'D')
                )
            );
        ");

        // Add full-text search index for Images table
        // Searches across OriginalName (weight A), FileName (weight B), AltText (weight B), and Caption (weight C)
        migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS idx_images_search_vector
            ON ""Images"" USING GIN (
                (
                    setweight(to_tsvector('english', coalesce(""OriginalName"", '')), 'A') ||
                    setweight(to_tsvector('english', coalesce(""FileName"", '')), 'B') ||
                    setweight(to_tsvector('english', coalesce(""AltText"", '')), 'B') ||
                    setweight(to_tsvector('english', coalesce(""Caption"", '')), 'C')
                )
            );
        ");

        // Add additional indexes for common filter fields

        // Users - Add index for Role and IsActive combination (if not already exists)
        migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS idx_users_role_isactive
            ON ""Users"" (""Role"", ""IsActive"");
        ");

        // Users - Add index for LastLoginAt for date range queries
        migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS idx_users_lastloginat
            ON ""Users"" (""LastLoginAt"")
            WHERE ""LastLoginAt"" IS NOT NULL;
        ");

        // Pages - Add index for Status and PublishedAt
        migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS idx_pages_status
            ON ""Pages"" (""Status"");
        ");

        migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS idx_pages_publishedat
            ON ""Pages"" (""PublishedAt"")
            WHERE ""PublishedAt"" IS NOT NULL;
        ");

        // Pages - Add index for CreatedByUserId for filtering by author
        migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS idx_pages_createdbyuserid
            ON ""Pages"" (""CreatedByUserId"")
            WHERE ""CreatedByUserId"" IS NOT NULL;
        ");

        // Images - Add indexes for common image filters
        migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS idx_images_contenttype
            ON ""Images"" (""ContentType"");
        ");

        migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS idx_images_size
            ON ""Images"" (""Size"");
        ");

        migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS idx_images_dimensions
            ON ""Images"" (""Width"", ""Height"");
        ");

        // Images - Add index for UploadedByUserId
        migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS idx_images_uploadedbyuserid
            ON ""Images"" (""UploadedByUserId"")
            WHERE ""UploadedByUserId"" IS NOT NULL;
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop all indexes created in Up migration
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_users_search_vector;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_pages_search_vector;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_images_search_vector;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_users_role_isactive;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_users_lastloginat;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_pages_status;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_pages_publishedat;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_pages_createdbyuserid;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_images_contenttype;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_images_size;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_images_dimensions;");
        migrationBuilder.Sql(@"DROP INDEX IF EXISTS idx_images_uploadedbyuserid;");
    }
}

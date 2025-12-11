using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Infrastructure.Persistence.Migrations;

/// <summary>
/// Migration to add PostgreSQL helper functions for full-text search
/// </summary>
public partial class AddFullTextSearchFunctions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create function for tsvector concatenation (||operator wrapper)
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION concat_tsvectors(vector1 tsvector, vector2 tsvector)
            RETURNS tsvector
            LANGUAGE sql
            IMMUTABLE
            PARALLEL SAFE
            AS $$
                SELECT vector1 || vector2;
            $$;
        ");

        // Create function for tsvector matches tsquery (@@ operator wrapper)
        migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION tsvector_matches(vector tsvector, query tsquery)
            RETURNS boolean
            LANGUAGE sql
            IMMUTABLE
            PARALLEL SAFE
            AS $$
                SELECT vector @@ query;
            $$;
        ");

        // Create comment for documentation
        migrationBuilder.Sql(@"
            COMMENT ON FUNCTION concat_tsvectors(tsvector, tsvector) IS
            'Wrapper function for tsvector || operator to enable EF Core integration';
        ");

        migrationBuilder.Sql(@"
            COMMENT ON FUNCTION tsvector_matches(tsvector, tsquery) IS
            'Wrapper function for tsvector @@ tsquery operator to enable EF Core integration';
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS concat_tsvectors(tsvector, tsvector);");
        migrationBuilder.Sql("DROP FUNCTION IF EXISTS tsvector_matches(tsvector, tsquery);");
    }
}

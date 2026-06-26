using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsCollector.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddSearchDocuments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE sources
            ADD COLUMN IF NOT EXISTS "DefaultLanguage" character varying(8);

            ALTER TABLE news
            ADD COLUMN IF NOT EXISTS "Language" character varying(8);

            ALTER TABLE news
            ADD COLUMN IF NOT EXISTS "SearchIndexedAt" timestamp with time zone;

            CREATE INDEX IF NOT EXISTS "IX_news_SearchIndexedAt_Pending"
                ON news ("SearchIndexedAt")
                WHERE "SearchIndexedAt" IS NULL;

            CREATE TABLE IF NOT EXISTS search_documents (
                "Id" uuid NOT NULL,
                "DocumentType" character varying(32) NOT NULL,
                "EntityId" uuid NOT NULL,
                "Language" character varying(8) NOT NULL,
                "Title" character varying(1024) NOT NULL,
                "Body" text,
                "SourceName" character varying(256),
                "PublishedAt" timestamp with time zone,
                "SearchVector" tsvector,
                "UpdatedAt" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_search_documents" PRIMARY KEY ("Id")
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_search_documents_DocumentType_EntityId"
                ON search_documents ("DocumentType", "EntityId");

            CREATE INDEX IF NOT EXISTS "IX_search_documents_PublishedAt"
                ON search_documents ("PublishedAt");

            CREATE INDEX IF NOT EXISTS "IX_search_documents_SearchVector"
                ON search_documents USING GIN ("SearchVector");

            CREATE OR REPLACE FUNCTION update_search_document_vector()
            RETURNS trigger AS $$
            DECLARE
                config regconfig;
            BEGIN
                config := CASE NEW."Language"
                    WHEN 'Ru' THEN 'russian'::regconfig
                    ELSE 'english'::regconfig
                END;

                NEW."SearchVector" :=
                    setweight(to_tsvector(config, coalesce(NEW."Title", '')), 'A') ||
                    setweight(to_tsvector(config, coalesce(left(NEW."Body", 8000), '')), 'B');

                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            DROP TRIGGER IF EXISTS trg_search_documents_vector ON search_documents;

            CREATE TRIGGER trg_search_documents_vector
                BEFORE INSERT OR UPDATE OF "Language", "Title", "Body"
                ON search_documents
                FOR EACH ROW
                EXECUTE FUNCTION update_search_document_vector();

            UPDATE sources
            SET "DefaultLanguage" = 'Ru'
            WHERE "DefaultLanguage" IS NULL
              AND (
                    lower("Url") LIKE '%.ru%'
                 OR lower("Name") LIKE '%ria%'
                 OR lower("Name") LIKE '%тасс%'
                 OR lower("Name") LIKE '%рбк%'
              );
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TRIGGER IF EXISTS trg_search_documents_vector ON search_documents;
            DROP FUNCTION IF EXISTS update_search_document_vector();
            DROP TABLE IF EXISTS search_documents;
            DROP INDEX IF EXISTS "IX_news_SearchIndexedAt_Pending";
            ALTER TABLE news DROP COLUMN IF EXISTS "SearchIndexedAt";
            ALTER TABLE news DROP COLUMN IF EXISTS "Language";
            ALTER TABLE sources DROP COLUMN IF EXISTS "DefaultLanguage";
            """);
    }
}

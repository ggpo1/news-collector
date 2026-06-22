using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNamedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE news
                ADD COLUMN IF NOT EXISTS "EntitiesExtractedAt" timestamp with time zone;

                CREATE TABLE IF NOT EXISTS named_entities (
                    "Id" uuid NOT NULL,
                    "Name" character varying(256) NOT NULL,
                    "Type" character varying(32) NOT NULL,
                    "NormalizedKey" character varying(320) NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_named_entities" PRIMARY KEY ("Id")
                );

                CREATE TABLE IF NOT EXISTS news_entity_mentions (
                    "Id" uuid NOT NULL,
                    "NewsItemId" uuid NOT NULL,
                    "NamedEntityId" uuid NOT NULL,
                    "MentionText" character varying(256) NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_news_entity_mentions" PRIMARY KEY ("Id"),
                    CONSTRAINT "FK_news_entity_mentions_named_entities_NamedEntityId"
                        FOREIGN KEY ("NamedEntityId") REFERENCES named_entities ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_news_entity_mentions_news_NewsItemId"
                        FOREIGN KEY ("NewsItemId") REFERENCES news ("Id") ON DELETE CASCADE
                );

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_named_entities_NormalizedKey"
                ON named_entities ("NormalizedKey");

                CREATE INDEX IF NOT EXISTS "IX_named_entities_Name"
                ON named_entities ("Name");

                CREATE INDEX IF NOT EXISTS "IX_named_entities_Type"
                ON named_entities ("Type");

                CREATE INDEX IF NOT EXISTS "IX_news_entity_mentions_NamedEntityId"
                ON news_entity_mentions ("NamedEntityId");

                CREATE INDEX IF NOT EXISTS "IX_news_entity_mentions_NewsItemId"
                ON news_entity_mentions ("NewsItemId");

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_news_entity_mentions_NewsItemId_NamedEntityId"
                ON news_entity_mentions ("NewsItemId", "NamedEntityId");

                CREATE INDEX IF NOT EXISTS "IX_news_FetchedAt_EntitiesPending"
                ON news ("FetchedAt")
                WHERE "EntitiesExtractedAt" IS NULL;

                CREATE INDEX IF NOT EXISTS "IX_news_FetchedAt_TonePending"
                ON news ("FetchedAt")
                WHERE "ToneCoefficient" IS NULL;

                CREATE INDEX IF NOT EXISTS "IX_news_FetchedAt_CategoryPending"
                ON news ("FetchedAt")
                WHERE "CategoryId" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "news_entity_mentions");

            migrationBuilder.DropTable(
                name: "named_entities");

            migrationBuilder.DropIndex(
                name: "IX_news_FetchedAt_EntitiesPending",
                table: "news");

            migrationBuilder.DropColumn(
                name: "EntitiesExtractedAt",
                table: "news");
        }
    }
}

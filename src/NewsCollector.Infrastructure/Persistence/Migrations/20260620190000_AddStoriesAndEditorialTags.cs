using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsCollector.Infrastructure.Persistence.Migrations
{
    public partial class AddStoriesAndEditorialTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE news
                ADD COLUMN IF NOT EXISTS "IsCategoryManual" boolean NOT NULL DEFAULT false;

                ALTER TABLE news
                ADD COLUMN IF NOT EXISTS "CategoryUpdatedAt" timestamp with time zone;

                ALTER TABLE news
                ADD COLUMN IF NOT EXISTS "CategoryUpdatedByUserId" uuid;

                DROP INDEX IF EXISTS "IX_news_FetchedAt_CategoryPending";

                CREATE INDEX IF NOT EXISTS "IX_news_FetchedAt_CategoryPending"
                ON news ("FetchedAt")
                WHERE "CategoryId" IS NULL AND "IsCategoryManual" = false;

                CREATE TABLE IF NOT EXISTS stories (
                    "Id" uuid NOT NULL,
                    "ClusterKey" character varying(64) NOT NULL,
                    "Title" character varying(1024) NOT NULL,
                    "Status" character varying(32) NOT NULL,
                    "PrimaryNewsItemId" uuid,
                    "ArticleCount" integer NOT NULL,
                    "SourceCount" integer NOT NULL,
                    "FirstSeenAt" timestamp with time zone,
                    "LastActivityAt" timestamp with time zone,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone NOT NULL,
                    "StatusUpdatedByUserId" uuid,
                    "StatusUpdatedAt" timestamp with time zone,
                    CONSTRAINT "PK_stories" PRIMARY KEY ("Id"),
                    CONSTRAINT "FK_stories_news_PrimaryNewsItemId" FOREIGN KEY ("PrimaryNewsItemId")
                        REFERENCES news ("Id") ON DELETE SET NULL,
                    CONSTRAINT "FK_stories_users_StatusUpdatedByUserId" FOREIGN KEY ("StatusUpdatedByUserId")
                        REFERENCES users ("Id") ON DELETE SET NULL
                );

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_stories_ClusterKey" ON stories ("ClusterKey");
                CREATE INDEX IF NOT EXISTS "IX_stories_Status" ON stories ("Status");
                CREATE INDEX IF NOT EXISTS "IX_stories_LastActivityAt" ON stories ("LastActivityAt");

                CREATE TABLE IF NOT EXISTS story_news_items (
                    "StoryId" uuid NOT NULL,
                    "NewsItemId" uuid NOT NULL,
                    "LinkedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_story_news_items" PRIMARY KEY ("StoryId", "NewsItemId"),
                    CONSTRAINT "FK_story_news_items_stories_StoryId" FOREIGN KEY ("StoryId")
                        REFERENCES stories ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_story_news_items_news_NewsItemId" FOREIGN KEY ("NewsItemId")
                        REFERENCES news ("Id") ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS "IX_story_news_items_NewsItemId" ON story_news_items ("NewsItemId");

                CREATE TABLE IF NOT EXISTS editorial_tags (
                    "Id" uuid NOT NULL,
                    "Slug" character varying(64) NOT NULL,
                    "Name" character varying(128) NOT NULL,
                    "Color" character varying(16),
                    "CreatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_editorial_tags" PRIMARY KEY ("Id")
                );

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_editorial_tags_Slug" ON editorial_tags ("Slug");

                CREATE TABLE IF NOT EXISTS news_editorial_tags (
                    "NewsItemId" uuid NOT NULL,
                    "EditorialTagId" uuid NOT NULL,
                    "AddedByUserId" uuid,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_news_editorial_tags" PRIMARY KEY ("NewsItemId", "EditorialTagId"),
                    CONSTRAINT "FK_news_editorial_tags_news_NewsItemId" FOREIGN KEY ("NewsItemId")
                        REFERENCES news ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_news_editorial_tags_editorial_tags_EditorialTagId" FOREIGN KEY ("EditorialTagId")
                        REFERENCES editorial_tags ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_news_editorial_tags_users_AddedByUserId" FOREIGN KEY ("AddedByUserId")
                        REFERENCES users ("Id") ON DELETE SET NULL
                );

                CREATE INDEX IF NOT EXISTS "IX_news_editorial_tags_EditorialTagId" ON news_editorial_tags ("EditorialTagId");

                INSERT INTO editorial_tags ("Id", "Slug", "Name", "Color", "CreatedAt")
                SELECT * FROM (VALUES
                    ('11111111-1111-1111-1111-111111111101'::uuid, 'srochno', '#срочно', '#e85d04', NOW()),
                    ('11111111-1111-1111-1111-111111111102'::uuid, 'needs-factcheck', '#needs-factcheck', '#7209b7', NOW()),
                    ('11111111-1111-1111-1111-111111111103'::uuid, 'for-channel', '#для-канала', '#4361ee', NOW()),
                    ('11111111-1111-1111-1111-111111111104'::uuid, 'in-work', '#в-работе', '#2a9d8f', NOW())
                ) AS seed("Id", "Slug", "Name", "Color", "CreatedAt")
                WHERE NOT EXISTS (SELECT 1 FROM editorial_tags LIMIT 1);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "news_editorial_tags");
            migrationBuilder.DropTable(name: "story_news_items");
            migrationBuilder.DropTable(name: "editorial_tags");
            migrationBuilder.DropTable(name: "stories");

            migrationBuilder.DropColumn(name: "IsCategoryManual", table: "news");
            migrationBuilder.DropColumn(name: "CategoryUpdatedAt", table: "news");
            migrationBuilder.DropColumn(name: "CategoryUpdatedByUserId", table: "news");
        }
    }
}

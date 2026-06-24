using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbeddingsStoriesAndEditorialTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CategoryUpdatedAt",
                table: "news",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryUpdatedByUserId",
                table: "news",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCategoryManual",
                table: "news",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "editorial_tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Color = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_editorial_tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "news_item_embeddings",
                columns: table => new
                {
                    NewsItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Model = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Vector = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_news_item_embeddings", x => x.NewsItemId);
                    table.ForeignKey(
                        name: "FK_news_item_embeddings_news_NewsItemId",
                        column: x => x.NewsItemId,
                        principalTable: "news",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClusterKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PrimaryNewsItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArticleCount = table.Column<int>(type: "integer", nullable: false),
                    SourceCount = table.Column<int>(type: "integer", nullable: false),
                    FirstSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastActivityAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StatusUpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    StatusUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stories_news_PrimaryNewsItemId",
                        column: x => x.PrimaryNewsItemId,
                        principalTable: "news",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_stories_users_StatusUpdatedByUserId",
                        column: x => x.StatusUpdatedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "news_editorial_tags",
                columns: table => new
                {
                    NewsItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    EditorialTagId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_news_editorial_tags", x => new { x.NewsItemId, x.EditorialTagId });
                    table.ForeignKey(
                        name: "FK_news_editorial_tags_editorial_tags_EditorialTagId",
                        column: x => x.EditorialTagId,
                        principalTable: "editorial_tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_news_editorial_tags_news_NewsItemId",
                        column: x => x.NewsItemId,
                        principalTable: "news",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_news_editorial_tags_users_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "story_news_items",
                columns: table => new
                {
                    StoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    NewsItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_story_news_items", x => new { x.StoryId, x.NewsItemId });
                    table.ForeignKey(
                        name: "FK_story_news_items_news_NewsItemId",
                        column: x => x.NewsItemId,
                        principalTable: "news",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_story_news_items_stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_editorial_tags_Slug",
                table: "editorial_tags",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_news_editorial_tags_AddedByUserId",
                table: "news_editorial_tags",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_news_editorial_tags_EditorialTagId",
                table: "news_editorial_tags",
                column: "EditorialTagId");

            migrationBuilder.CreateIndex(
                name: "IX_news_item_embeddings_Model",
                table: "news_item_embeddings",
                column: "Model");

            migrationBuilder.CreateIndex(
                name: "IX_stories_ClusterKey",
                table: "stories",
                column: "ClusterKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stories_LastActivityAt",
                table: "stories",
                column: "LastActivityAt");

            migrationBuilder.CreateIndex(
                name: "IX_stories_PrimaryNewsItemId",
                table: "stories",
                column: "PrimaryNewsItemId");

            migrationBuilder.CreateIndex(
                name: "IX_stories_Status",
                table: "stories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_stories_StatusUpdatedByUserId",
                table: "stories",
                column: "StatusUpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_story_news_items_NewsItemId",
                table: "story_news_items",
                column: "NewsItemId");

            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_news_FetchedAt_CategoryPending";

                CREATE INDEX IF NOT EXISTS "IX_news_FetchedAt_CategoryPending"
                ON news ("FetchedAt")
                WHERE "CategoryId" IS NULL AND "IsCategoryManual" = false;

                CREATE INDEX IF NOT EXISTS "IX_news_ToneCoefficient"
                ON news ("ToneCoefficient");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "news_editorial_tags");

            migrationBuilder.DropTable(
                name: "news_item_embeddings");

            migrationBuilder.DropTable(
                name: "story_news_items");

            migrationBuilder.DropTable(
                name: "editorial_tags");

            migrationBuilder.DropTable(
                name: "stories");

            migrationBuilder.DropColumn(
                name: "CategoryUpdatedAt",
                table: "news");

            migrationBuilder.DropColumn(
                name: "CategoryUpdatedByUserId",
                table: "news");

            migrationBuilder.DropColumn(
                name: "IsCategoryManual",
                table: "news");
        }
    }
}

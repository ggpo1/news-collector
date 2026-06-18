using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FetchIntervalMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    LastFetchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Config = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "news",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Title = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: true),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FetchedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ContentHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RawPayload = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_news", x => x.Id);
                    table.ForeignKey(
                        name: "FK_news_sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "news_links",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NewsIdLow = table.Column<Guid>(type: "uuid", nullable: false),
                    NewsIdHigh = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LinkMethod = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_news_links", x => x.Id);
                    table.CheckConstraint("CK_news_links_ordered_ids", "\"NewsIdLow\" < \"NewsIdHigh\"");
                    table.ForeignKey(
                        name: "FK_news_links_news_NewsIdHigh",
                        column: x => x.NewsIdHigh,
                        principalTable: "news",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_news_links_news_NewsIdLow",
                        column: x => x.NewsIdLow,
                        principalTable: "news",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "sources",
                columns: new[] { "Id", "Config", "CreatedAt", "FetchIntervalMinutes", "IsActive", "LastFetchedAt", "Name", "Type", "UpdatedAt", "Url" },
                values: new object[] { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), null, new DateTimeOffset(new DateTime(2026, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 15, true, null, "РИА Новости", "Rss", new DateTimeOffset(new DateTime(2026, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "https://ria.ru/export/rss2/archive/index.xml" });

            migrationBuilder.CreateIndex(
                name: "IX_news_FetchedAt",
                table: "news",
                column: "FetchedAt");

            migrationBuilder.CreateIndex(
                name: "IX_news_SourceId_ExternalId",
                table: "news",
                columns: new[] { "SourceId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_news_SourceId_PublishedAt",
                table: "news",
                columns: new[] { "SourceId", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_news_SourceId_Url",
                table: "news",
                columns: new[] { "SourceId", "Url" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_news_links_NewsIdHigh",
                table: "news_links",
                column: "NewsIdHigh");

            migrationBuilder.CreateIndex(
                name: "IX_news_links_NewsIdLow",
                table: "news_links",
                column: "NewsIdLow");

            migrationBuilder.CreateIndex(
                name: "IX_news_links_NewsIdLow_NewsIdHigh",
                table: "news_links",
                columns: new[] { "NewsIdLow", "NewsIdHigh" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sources_IsActive",
                table: "sources",
                column: "IsActive",
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_sources_Url",
                table: "sources",
                column: "Url",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "news_links");

            migrationBuilder.DropTable(
                name: "news");

            migrationBuilder.DropTable(
                name: "sources");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "telegram_bots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BotToken = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ContainerName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ContainerStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ContainerError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telegram_bots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "telegram_channels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TelegramBotId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ChatId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telegram_channels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_telegram_channels_telegram_bots_TelegramBotId",
                        column: x => x.TelegramBotId,
                        principalTable: "telegram_bots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "telegram_channel_categories",
                columns: table => new
                {
                    TelegramChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telegram_channel_categories", x => new { x.TelegramChannelId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_telegram_channel_categories_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_telegram_channel_categories_telegram_channels_TelegramChann~",
                        column: x => x.TelegramChannelId,
                        principalTable: "telegram_channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "telegram_channel_sources",
                columns: table => new
                {
                    TelegramChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telegram_channel_sources", x => new { x.TelegramChannelId, x.SourceId });
                    table.ForeignKey(
                        name: "FK_telegram_channel_sources_sources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "sources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_telegram_channel_sources_telegram_channels_TelegramChannelId",
                        column: x => x.TelegramChannelId,
                        principalTable: "telegram_channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "telegram_deliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TelegramChannelId = table.Column<Guid>(type: "uuid", nullable: false),
                    NewsItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewsRewriteId = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageText = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TelegramMessageId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telegram_deliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_telegram_deliveries_news_NewsItemId",
                        column: x => x.NewsItemId,
                        principalTable: "news",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_telegram_deliveries_news_rewrites_NewsRewriteId",
                        column: x => x.NewsRewriteId,
                        principalTable: "news_rewrites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_telegram_deliveries_telegram_channels_TelegramChannelId",
                        column: x => x.TelegramChannelId,
                        principalTable: "telegram_channels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_telegram_bots_Name",
                table: "telegram_bots",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_telegram_channel_categories_CategoryId",
                table: "telegram_channel_categories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_telegram_channel_sources_SourceId",
                table: "telegram_channel_sources",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_telegram_channels_TelegramBotId_ChatId",
                table: "telegram_channels",
                columns: new[] { "TelegramBotId", "ChatId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_telegram_deliveries_NewsItemId",
                table: "telegram_deliveries",
                column: "NewsItemId");

            migrationBuilder.CreateIndex(
                name: "IX_telegram_deliveries_NewsRewriteId",
                table: "telegram_deliveries",
                column: "NewsRewriteId");

            migrationBuilder.CreateIndex(
                name: "IX_telegram_deliveries_Status_CreatedAt",
                table: "telegram_deliveries",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_telegram_deliveries_TelegramChannelId",
                table: "telegram_deliveries",
                column: "TelegramChannelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "telegram_channel_categories");
            migrationBuilder.DropTable(name: "telegram_channel_sources");
            migrationBuilder.DropTable(name: "telegram_deliveries");
            migrationBuilder.DropTable(name: "telegram_channels");
            migrationBuilder.DropTable(name: "telegram_bots");
        }
    }
}

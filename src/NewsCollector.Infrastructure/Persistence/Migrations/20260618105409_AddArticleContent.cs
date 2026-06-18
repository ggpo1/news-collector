using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "news",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ContentFetchedAt",
                table: "news",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "sources",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                column: "Config",
                value: "{\"contentFetchEnabled\":true,\"contentSelector\":\".article__text\"}");

            migrationBuilder.CreateIndex(
                name: "IX_news_ContentFetchedAt",
                table: "news",
                column: "ContentFetchedAt",
                filter: "\"Content\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_news_ContentFetchedAt",
                table: "news");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "news");

            migrationBuilder.DropColumn(
                name: "ContentFetchedAt",
                table: "news");

            migrationBuilder.UpdateData(
                table: "sources",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                column: "Config",
                value: null);
        }
    }
}

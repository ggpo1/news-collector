using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsToneCoefficient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ToneCoefficient",
                table: "news",
                type: "numeric(4,3)",
                precision: 4,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ToneAnalyzedAt",
                table: "news",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_news_FetchedAt_ToneCoefficient",
                table: "news",
                column: "FetchedAt",
                filter: "\"ToneCoefficient\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_news_FetchedAt_ToneCoefficient",
                table: "news");

            migrationBuilder.DropColumn(
                name: "ToneCoefficient",
                table: "news");

            migrationBuilder.DropColumn(
                name: "ToneAnalyzedAt",
                table: "news");
        }
    }
}

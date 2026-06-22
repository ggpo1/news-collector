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
            migrationBuilder.Sql("""
                ALTER TABLE news
                ADD COLUMN IF NOT EXISTS "ToneCoefficient" numeric(4,3);

                ALTER TABLE news
                ADD COLUMN IF NOT EXISTS "ToneAnalyzedAt" timestamp with time zone;

                CREATE INDEX IF NOT EXISTS "IX_news_FetchedAt_ToneCoefficient"
                ON news ("FetchedAt")
                WHERE "ToneCoefficient" IS NULL;
                """);
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

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsItemEmbeddings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS news_item_embeddings (
                    "NewsItemId" uuid NOT NULL,
                    "Model" character varying(128) NOT NULL,
                    "Vector" jsonb NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_news_item_embeddings" PRIMARY KEY ("NewsItemId"),
                    CONSTRAINT "FK_news_item_embeddings_news_NewsItemId" FOREIGN KEY ("NewsItemId")
                        REFERENCES news ("Id") ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS "IX_news_item_embeddings_Model"
                ON news_item_embeddings ("Model");

                CREATE INDEX IF NOT EXISTS "IX_news_ToneCoefficient"
                ON news ("ToneCoefficient");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "news_item_embeddings");

            migrationBuilder.DropIndex(
                name: "IX_news_ToneCoefficient",
                table: "news");
        }
    }
}

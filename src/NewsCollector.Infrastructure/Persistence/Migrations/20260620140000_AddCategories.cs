using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NewsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "Name", "Slug", "SortOrder" },
                values: new object[,]
                {
                    { new Guid("c1000001-0000-4000-8000-00000000000a"), new DateTimeOffset(new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Прочие новости, не подходящие под другие категории", true, "Другое", "other", 100 },
                    { new Guid("c1000001-0000-4000-8000-000000000001"), new DateTimeOffset(new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Государство, выборы, законы, политические заявления", true, "Политика", "politics", 10 },
                    { new Guid("c1000001-0000-4000-8000-000000000002"), new DateTimeOffset(new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Финансы, бизнес, рынки, налоги, бюджет", true, "Экономика", "economy", 20 },
                    { new Guid("c1000001-0000-4000-8000-000000000003"), new DateTimeOffset(new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Социальные темы, образование, здравоохранение, демография", true, "Общество", "society", 30 },
                    { new Guid("c1000001-0000-4000-8000-000000000004"), new DateTimeOffset(new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Международные события и внешняя политика", true, "В мире", "world", 40 },
                    { new Guid("c1000001-0000-4000-8000-000000000005"), new DateTimeOffset(new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "IT, интернет, гаджеты, цифровизация", true, "Технологии", "tech", 50 },
                    { new Guid("c1000001-0000-4000-8000-000000000006"), new DateTimeOffset(new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Научные открытия, исследования, космос", true, "Наука", "science", 60 },
                    { new Guid("c1000001-0000-4000-8000-000000000007"), new DateTimeOffset(new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Спортивные события, соревнования, трансферы", true, "Спорт", "sports", 70 },
                    { new Guid("c1000001-0000-4000-8000-000000000008"), new DateTimeOffset(new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Искусство, кино, музыка, литература", true, "Культура", "culture", 80 },
                    { new Guid("c1000001-0000-4000-8000-000000000009"), new DateTimeOffset(new DateTime(2026, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Аварии, преступления, ЧП, катастрофы", true, "Происшествия", "incidents", 90 }
                });

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "news",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_IsActive",
                table: "categories",
                column: "IsActive",
                filter: "\"IsActive\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_categories_Slug",
                table: "categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_news_CategoryId",
                table: "news",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_news_FetchedAt_CategoryId",
                table: "news",
                column: "FetchedAt",
                filter: "\"CategoryId\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_news_categories_CategoryId",
                table: "news",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_news_categories_CategoryId",
                table: "news");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropIndex(
                name: "IX_news_CategoryId",
                table: "news");

            migrationBuilder.DropIndex(
                name: "IX_news_FetchedAt_CategoryId",
                table: "news");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "news");
        }
    }
}

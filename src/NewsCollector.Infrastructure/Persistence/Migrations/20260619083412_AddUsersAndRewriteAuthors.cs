using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndRewriteAuthors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Login = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                table: "news_rewrites",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_news_rewrites_AuthorId",
                table: "news_rewrites",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_news_rewrites_SourceNewsId_AuthorId",
                table: "news_rewrites",
                columns: new[] { "SourceNewsId", "AuthorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Login",
                table: "users",
                column: "Login",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_news_rewrites_users_AuthorId",
                table: "news_rewrites",
                column: "AuthorId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_news_rewrites_users_AuthorId",
                table: "news_rewrites");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropIndex(
                name: "IX_news_rewrites_AuthorId",
                table: "news_rewrites");

            migrationBuilder.DropIndex(
                name: "IX_news_rewrites_SourceNewsId_AuthorId",
                table: "news_rewrites");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "news_rewrites");
        }
    }
}

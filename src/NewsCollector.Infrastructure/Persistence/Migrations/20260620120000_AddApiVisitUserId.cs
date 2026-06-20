using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddApiVisitUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "api_visits",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_api_visits_UserId",
                table: "api_visits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_api_visits_UserId_RequestedAt",
                table: "api_visits",
                columns: new[] { "UserId", "RequestedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_api_visits_users_UserId",
                table: "api_visits",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_api_visits_users_UserId",
                table: "api_visits");

            migrationBuilder.DropIndex(
                name: "IX_api_visits_UserId",
                table: "api_visits");

            migrationBuilder.DropIndex(
                name: "IX_api_visits_UserId_RequestedAt",
                table: "api_visits");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "api_visits");
        }
    }
}

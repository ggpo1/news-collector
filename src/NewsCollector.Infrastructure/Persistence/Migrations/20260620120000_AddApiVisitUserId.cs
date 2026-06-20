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
            // Idempotent: column may already exist if migration was partially applied manually.
            migrationBuilder.Sql("""
                ALTER TABLE api_visits
                ADD COLUMN IF NOT EXISTS "UserId" uuid;

                CREATE INDEX IF NOT EXISTS "IX_api_visits_UserId"
                ON api_visits ("UserId");

                CREATE INDEX IF NOT EXISTS "IX_api_visits_UserId_RequestedAt"
                ON api_visits ("UserId", "RequestedAt");

                DO $EF$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'FK_api_visits_users_UserId'
                    ) THEN
                        ALTER TABLE api_visits
                        ADD CONSTRAINT "FK_api_visits_users_UserId"
                        FOREIGN KEY ("UserId") REFERENCES users ("Id")
                        ON DELETE SET NULL;
                    END IF;
                END $EF$;
                """);
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

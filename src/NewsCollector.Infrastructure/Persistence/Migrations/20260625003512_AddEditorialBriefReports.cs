using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsCollector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEditorialBriefReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS editorial_brief_reports (
                    "Id" uuid NOT NULL,
                    "Period" character varying(16) NOT NULL,
                    "Markdown" text NOT NULL,
                    "GeneratedAt" timestamp with time zone NOT NULL,
                    "WindowStart" timestamp with time zone NOT NULL,
                    "WindowEnd" timestamp with time zone NOT NULL,
                    "Model" character varying(128),
                    CONSTRAINT "PK_editorial_brief_reports" PRIMARY KEY ("Id")
                );

                CREATE INDEX IF NOT EXISTS "IX_editorial_brief_reports_GeneratedAt"
                ON editorial_brief_reports ("GeneratedAt");

                CREATE INDEX IF NOT EXISTS "IX_editorial_brief_reports_Period_GeneratedAt"
                ON editorial_brief_reports ("Period", "GeneratedAt");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "editorial_brief_reports");
        }
    }
}

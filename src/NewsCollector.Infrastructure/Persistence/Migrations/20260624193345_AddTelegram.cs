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
            migrationBuilder.Sql("""
                DO $migration$
                DECLARE
                    table_name text;
                BEGIN
                    FOREACH table_name IN ARRAY ARRAY[
                        'telegram_deliveries',
                        'telegram_channel_sources',
                        'telegram_channel_categories',
                        'telegram_channels',
                        'telegram_bots'
                    ] LOOP
                        IF NOT EXISTS (
                            SELECT 1 FROM pg_tables
                            WHERE schemaname = 'public' AND tablename = table_name
                        ) AND EXISTS (
                            SELECT 1 FROM pg_type ty
                            JOIN pg_namespace n ON n.oid = ty.typnamespace
                            WHERE n.nspname = 'public' AND ty.typname = table_name
                        ) THEN
                            EXECUTE format('DROP TYPE %I', table_name);
                        END IF;
                    END LOOP;
                END
                $migration$;

                CREATE TABLE IF NOT EXISTS telegram_bots (
                    "Id" uuid NOT NULL,
                    "Name" character varying(200) NOT NULL,
                    "BotToken" character varying(256) NOT NULL,
                    "IsActive" boolean NOT NULL,
                    "ContainerName" character varying(128),
                    "ContainerStatus" character varying(32) NOT NULL,
                    "ContainerError" character varying(2000),
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_telegram_bots" PRIMARY KEY ("Id")
                );

                CREATE TABLE IF NOT EXISTS telegram_channels (
                    "Id" uuid NOT NULL,
                    "TelegramBotId" uuid NOT NULL,
                    "Name" character varying(200) NOT NULL,
                    "ChatId" character varying(128) NOT NULL,
                    "IsActive" boolean NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_telegram_channels" PRIMARY KEY ("Id"),
                    CONSTRAINT "FK_telegram_channels_telegram_bots_TelegramBotId"
                        FOREIGN KEY ("TelegramBotId") REFERENCES telegram_bots ("Id") ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS telegram_channel_categories (
                    "TelegramChannelId" uuid NOT NULL,
                    "CategoryId" uuid NOT NULL,
                    CONSTRAINT "PK_telegram_channel_categories" PRIMARY KEY ("TelegramChannelId", "CategoryId"),
                    CONSTRAINT "FK_telegram_channel_categories_categories_CategoryId"
                        FOREIGN KEY ("CategoryId") REFERENCES categories ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_telegram_channel_categories_telegram_channels_TelegramChann~"
                        FOREIGN KEY ("TelegramChannelId") REFERENCES telegram_channels ("Id") ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS telegram_channel_sources (
                    "TelegramChannelId" uuid NOT NULL,
                    "SourceId" uuid NOT NULL,
                    CONSTRAINT "PK_telegram_channel_sources" PRIMARY KEY ("TelegramChannelId", "SourceId"),
                    CONSTRAINT "FK_telegram_channel_sources_sources_SourceId"
                        FOREIGN KEY ("SourceId") REFERENCES sources ("Id") ON DELETE CASCADE,
                    CONSTRAINT "FK_telegram_channel_sources_telegram_channels_TelegramChannelId"
                        FOREIGN KEY ("TelegramChannelId") REFERENCES telegram_channels ("Id") ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS telegram_deliveries (
                    "Id" uuid NOT NULL,
                    "TelegramChannelId" uuid NOT NULL,
                    "NewsItemId" uuid,
                    "NewsRewriteId" uuid,
                    "MessageText" text NOT NULL,
                    "Status" character varying(32) NOT NULL,
                    "ErrorMessage" character varying(2000),
                    "TelegramMessageId" bigint,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "SentAt" timestamp with time zone,
                    CONSTRAINT "PK_telegram_deliveries" PRIMARY KEY ("Id"),
                    CONSTRAINT "FK_telegram_deliveries_news_NewsItemId"
                        FOREIGN KEY ("NewsItemId") REFERENCES news ("Id") ON DELETE SET NULL,
                    CONSTRAINT "FK_telegram_deliveries_news_rewrites_NewsRewriteId"
                        FOREIGN KEY ("NewsRewriteId") REFERENCES news_rewrites ("Id") ON DELETE SET NULL,
                    CONSTRAINT "FK_telegram_deliveries_telegram_channels_TelegramChannelId"
                        FOREIGN KEY ("TelegramChannelId") REFERENCES telegram_channels ("Id") ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS "IX_telegram_bots_Name"
                ON telegram_bots ("Name");

                CREATE INDEX IF NOT EXISTS "IX_telegram_channel_categories_CategoryId"
                ON telegram_channel_categories ("CategoryId");

                CREATE INDEX IF NOT EXISTS "IX_telegram_channel_sources_SourceId"
                ON telegram_channel_sources ("SourceId");

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_telegram_channels_TelegramBotId_ChatId"
                ON telegram_channels ("TelegramBotId", "ChatId");

                CREATE INDEX IF NOT EXISTS "IX_telegram_deliveries_NewsItemId"
                ON telegram_deliveries ("NewsItemId");

                CREATE INDEX IF NOT EXISTS "IX_telegram_deliveries_NewsRewriteId"
                ON telegram_deliveries ("NewsRewriteId");

                CREATE INDEX IF NOT EXISTS "IX_telegram_deliveries_Status_CreatedAt"
                ON telegram_deliveries ("Status", "CreatedAt");

                CREATE INDEX IF NOT EXISTS "IX_telegram_deliveries_TelegramChannelId"
                ON telegram_deliveries ("TelegramChannelId");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "telegram_channel_categories");

            migrationBuilder.DropTable(
                name: "telegram_channel_sources");

            migrationBuilder.DropTable(
                name: "telegram_deliveries");

            migrationBuilder.DropTable(
                name: "telegram_channels");

            migrationBuilder.DropTable(
                name: "telegram_bots");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventbox.EventManagement.EventApi.Migrations
{
    /// <inheritdoc />
    public partial class FixOutboxColumnTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'Outboxes'
                          AND column_name = 'IntergrationEventType'
                    ) THEN
                        ALTER TABLE "Outboxes" RENAME COLUMN "IntergrationEventType" TO "IntegrationEventType";
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IntegrationEventType",
                table: "Outboxes",
                newName: "IntergrationEventType");
        }
    }
}

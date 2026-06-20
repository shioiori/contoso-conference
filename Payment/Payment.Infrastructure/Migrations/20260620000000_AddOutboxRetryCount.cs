using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventbox.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxRetryCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE "Outboxes" ADD COLUMN IF NOT EXISTS "RetryCount" integer NOT NULL DEFAULT 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "Outboxes");
        }
    }
}

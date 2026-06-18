using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventbox.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixOutboxColumnTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IntergrationEventType",
                table: "Outboxes",
                newName: "IntegrationEventType");
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

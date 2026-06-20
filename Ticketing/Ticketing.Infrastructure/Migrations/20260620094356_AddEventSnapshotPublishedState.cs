using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventbox.Ticketing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventSnapshotPublishedState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "EventSchedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "EventSchedules");
        }
    }
}

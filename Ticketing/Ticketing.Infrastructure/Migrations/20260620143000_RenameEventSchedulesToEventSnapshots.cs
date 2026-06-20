using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventbox.Ticketing.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(TicketingDbContext))]
    [Migration("20260620143000_RenameEventSchedulesToEventSnapshots")]
    public partial class RenameEventSchedulesToEventSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EventSchedules",
                table: "EventSchedules");

            migrationBuilder.RenameTable(
                name: "EventSchedules",
                newName: "EventSnapshots");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventSnapshots",
                table: "EventSnapshots",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EventSnapshots",
                table: "EventSnapshots");

            migrationBuilder.RenameTable(
                name: "EventSnapshots",
                newName: "EventSchedules");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventSchedules",
                table: "EventSchedules",
                column: "Id");
        }
    }
}

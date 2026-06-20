using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Eventbox.EventManagement.EventApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesAndAddPricingPhase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop FK first — it references PK_Events, so PK cannot be dropped while FK exists
            migrationBuilder.DropForeignKey(
                name: "FK_TicketTypes_Events_EventId",
                table: "TicketTypes");

            // Rename Events → Event
            migrationBuilder.DropPrimaryKey(
                name: "PK_Events",
                table: "Events");

            migrationBuilder.RenameTable(
                name: "Events",
                newName: "Event");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Event",
                table: "Event",
                column: "Id");

            // Rename Organizations → Organization
            migrationBuilder.DropPrimaryKey(
                name: "PK_Organizations",
                table: "Organizations");

            migrationBuilder.RenameTable(
                name: "Organizations",
                newName: "Organization");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Organization",
                table: "Organization",
                column: "Id");

            // Rename TicketTypes → TicketType
            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketTypes",
                table: "TicketTypes");

            migrationBuilder.RenameTable(
                name: "TicketTypes",
                newName: "TicketType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketType",
                table: "TicketType",
                column: "Id");

            // Add new columns to TicketType
            migrationBuilder.AddColumn<string>(
                name: "AccessCodeHash",
                table: "TicketType",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "TicketType",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TicketType",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPerOrder",
                table: "TicketType",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinPerOrder",
                table: "TicketType",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                table: "TicketType",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Re-add FK pointing to renamed Event table
            migrationBuilder.AddForeignKey(
                name: "FK_TicketType_Event_EventId",
                table: "TicketType",
                column: "EventId",
                principalTable: "Event",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            // Create PricingPhase table
            migrationBuilder.CreateTable(
                name: "PricingPhase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TicketTypeId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    UpdatedAt = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingPhase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PricingPhase_TicketType_TicketTypeId",
                        column: x => x.TicketTypeId,
                        principalTable: "TicketType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PricingPhase_TicketTypeId",
                table: "PricingPhase",
                column: "TicketTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingPhase");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketType_Event_EventId",
                table: "TicketType");

            migrationBuilder.DropColumn(name: "AccessCodeHash", table: "TicketType");
            migrationBuilder.DropColumn(name: "Currency", table: "TicketType");
            migrationBuilder.DropColumn(name: "Description", table: "TicketType");
            migrationBuilder.DropColumn(name: "MaxPerOrder", table: "TicketType");
            migrationBuilder.DropColumn(name: "MinPerOrder", table: "TicketType");
            migrationBuilder.DropColumn(name: "Visibility", table: "TicketType");

            migrationBuilder.DropPrimaryKey(name: "PK_TicketType", table: "TicketType");
            migrationBuilder.RenameTable(name: "TicketType", newName: "TicketTypes");
            migrationBuilder.AddPrimaryKey(name: "PK_TicketTypes", table: "TicketTypes", column: "Id");

            migrationBuilder.DropPrimaryKey(name: "PK_Organization", table: "Organization");
            migrationBuilder.RenameTable(name: "Organization", newName: "Organizations");
            migrationBuilder.AddPrimaryKey(name: "PK_Organizations", table: "Organizations", column: "Id");

            migrationBuilder.DropPrimaryKey(name: "PK_Event", table: "Event");
            migrationBuilder.RenameTable(name: "Event", newName: "Events");
            migrationBuilder.AddPrimaryKey(name: "PK_Events", table: "Events", column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketTypes_Events_EventId",
                table: "TicketTypes",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

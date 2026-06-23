using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventbox.Ticketing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTicketAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PricingPhaseAvailability_TicketTypeAvailability_TicketTypeA~",
                table: "PricingPhaseAvailability");

            migrationBuilder.DropTable(
                name: "TicketTypeAvailability");

            migrationBuilder.DropTable(
                name: "TicketAvailabilities");

            migrationBuilder.CreateTable(
                name: "TicketTypeAvailabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Remaining = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    MinPerOrder = table.Column<int>(type: "integer", nullable: false),
                    MaxPerOrder = table.Column<int>(type: "integer", nullable: true),
                    Visibility = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccessCodeHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTypeAvailabilities", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypeAvailabilities_EventId",
                table: "TicketTypeAvailabilities",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_PricingPhaseAvailability_TicketTypeAvailabilities_TicketTyp~",
                table: "PricingPhaseAvailability",
                column: "TicketTypeAvailabilityId",
                principalTable: "TicketTypeAvailabilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PricingPhaseAvailability_TicketTypeAvailabilities_TicketTyp~",
                table: "PricingPhaseAvailability");

            migrationBuilder.DropTable(
                name: "TicketTypeAvailabilities");

            migrationBuilder.CreateTable(
                name: "TicketAvailabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAvailabilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketTypeAvailability",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessCodeHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    MaxPerOrder = table.Column<int>(type: "integer", nullable: true),
                    MinPerOrder = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Remaining = table.Column<int>(type: "integer", nullable: false),
                    TicketAvailabilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Visibility = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTypeAvailability", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketTypeAvailability_TicketAvailabilities_TicketAvailabil~",
                        column: x => x.TicketAvailabilityId,
                        principalTable: "TicketAvailabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypeAvailability_TicketAvailabilityId",
                table: "TicketTypeAvailability",
                column: "TicketAvailabilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_PricingPhaseAvailability_TicketTypeAvailability_TicketTypeA~",
                table: "PricingPhaseAvailability",
                column: "TicketTypeAvailabilityId",
                principalTable: "TicketTypeAvailability",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

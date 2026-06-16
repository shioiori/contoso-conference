using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Eventbox.Ticketing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingDetailsToTicketing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccessCodeHash",
                table: "TicketTypeAvailability",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "TicketTypeAvailability",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "VND");

            migrationBuilder.AddColumn<int>(
                name: "MinPerOrder",
                table: "TicketTypeAvailability",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "MaxPerOrder",
                table: "TicketTypeAvailability",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TicketTypeAvailability",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                table: "TicketTypeAvailability",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Public");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "OrderItems",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "VND");

            migrationBuilder.AddColumn<int>(
                name: "PricingPhaseId",
                table: "OrderItems",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "PricingPhaseName",
                table: "OrderItems",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "Default");

            migrationBuilder.AddColumn<string>(
                name: "TicketTypeName",
                table: "OrderItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "OrderItems",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PricingPhaseAvailability",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TicketTypeAvailabilityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingPhaseAvailability", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PricingPhaseAvailability_TicketTypeAvailability_TicketTypeA~",
                        column: x => x.TicketTypeAvailabilityId,
                        principalTable: "TicketTypeAvailability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PricingPhaseAvailability_TicketTypeAvailabilityId",
                table: "PricingPhaseAvailability",
                column: "TicketTypeAvailabilityId");

            migrationBuilder.Sql(
                """
                UPDATE "TicketTypeAvailability"
                SET "Name" = 'Ticket type ' || "Id"
                WHERE "Name" = '';
                """);

            migrationBuilder.Sql(
                """
                UPDATE "OrderItems"
                SET "TicketTypeName" = 'Ticket type ' || "TicketTypeId"
                WHERE "TicketTypeName" = '';
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO "PricingPhaseAvailability" ("Name", "Price", "StartTime", "EndTime", "TicketTypeAvailabilityId")
                SELECT 'Default', 0, NULL, NULL, "Id"
                FROM "TicketTypeAvailability";
                """);

            migrationBuilder.DropColumn(
                name: "TicketTypeId",
                table: "TicketTypeAvailability");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingPhaseAvailability");

            migrationBuilder.DropColumn(
                name: "AccessCodeHash",
                table: "TicketTypeAvailability");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "TicketTypeAvailability");

            migrationBuilder.DropColumn(
                name: "MaxPerOrder",
                table: "TicketTypeAvailability");

            migrationBuilder.DropColumn(
                name: "MinPerOrder",
                table: "TicketTypeAvailability");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "TicketTypeAvailability");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "TicketTypeAvailability");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PricingPhaseId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PricingPhaseName",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TicketTypeName",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "OrderItems");

            migrationBuilder.AddColumn<int>(
                name: "TicketTypeId",
                table: "TicketTypeAvailability",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

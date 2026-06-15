using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventbox.EventManagement.EventApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameEventDateWindowToFromTo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "From",
                table: "Event",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "To",
                table: "Event",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.Sql(
                """
                UPDATE "Event"
                SET "From" = ("StartDate"::timestamp AT TIME ZONE 'UTC'),
                    "To" = ("EndDate"::timestamp AT TIME ZONE 'UTC')
                """);

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Event");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "Event",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "Event",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.Sql(
                """
                UPDATE "Event"
                SET "StartDate" = "From"::date,
                    "EndDate" = "To"::date
                """);

            migrationBuilder.DropColumn(
                name: "From",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "To",
                table: "Event");
        }
    }
}

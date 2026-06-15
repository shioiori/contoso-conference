using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventbox.EventManagement.EventApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditableEntityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "TicketType",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "TicketType",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "PricingPhase",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "PricingPhase",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Organization",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Organization",
                newName: "UpdatedDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Event",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Event",
                newName: "UpdatedDate");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "TicketType",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "TicketType",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "PricingPhase",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "PricingPhase",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "Organization",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Organization",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "Event",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Event",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateOnly>(
                name: "CreatedDate",
                table: "TicketType",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "UpdatedDate",
                table: "TicketType",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "CreatedDate",
                table: "PricingPhase",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "UpdatedDate",
                table: "PricingPhase",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "CreatedDate",
                table: "Organization",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "UpdatedDate",
                table: "Organization",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "CreatedDate",
                table: "Event",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "UpdatedDate",
                table: "Event",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "TicketType",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "TicketType",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "PricingPhase",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "PricingPhase",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Organization",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "Organization",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Event",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedDate",
                table: "Event",
                newName: "UpdatedAt");
        }
    }
}

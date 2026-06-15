using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventbox.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditableEntityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Payments",
                newName: "CreatedDate");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Payments",
                newName: "CreatedAt");
        }
    }
}

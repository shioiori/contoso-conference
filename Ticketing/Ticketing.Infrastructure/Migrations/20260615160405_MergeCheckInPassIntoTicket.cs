using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventbox.TicketingInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MergeCheckInPassIntoTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckInAttempts");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CheckedInAt",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CheckedInByUserId",
                table: "Tickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                table: "Tickets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "QrToken",
                table: "Tickets",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QrTokenHash",
                table: "Tickets",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Tickets" t
                SET "EventId" = o."EventId"
                FROM "Orders" o
                WHERE t."OrderId" = o."Id"
                """);

            migrationBuilder.Sql(
                """
                UPDATE "Tickets" t
                SET "QrToken" = p."QrToken",
                    "QrTokenHash" = p."QrTokenHash",
                    "CheckedInAt" = p."CheckedInAt",
                    "CheckedInByUserId" = p."CheckedInByUserId",
                    "TicketState" = CASE WHEN p."Status" = 3 THEN 2 ELSE t."TicketState" END
                FROM "CheckInPasses" p
                WHERE p."RegistrationTicketId" = t."Id"
                """);

            migrationBuilder.DropTable(
                name: "CheckInPasses");

            migrationBuilder.CreateTable(
                name: "EventSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    From = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    To = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSchedules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_EventId_TicketState",
                table: "Tickets",
                columns: new[] { "EventId", "TicketState" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_QrToken",
                table: "Tickets",
                column: "QrToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_QrTokenHash",
                table: "Tickets",
                column: "QrTokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_EventId_TicketState",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_QrToken",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_QrTokenHash",
                table: "Tickets");

            migrationBuilder.CreateTable(
                name: "CheckInPasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendeeEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    AttendeeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CheckedInAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CheckedInByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    QrToken = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    QrTokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RegistrationOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationTicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TicketTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckInPasses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CheckInAttempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckInPassId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    QrTokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    ScannedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StaffUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckInAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckInAttempts_CheckInPasses_CheckInPassId",
                        column: x => x.CheckInPassId,
                        principalTable: "CheckInPasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckInAttempts_CheckInPassId",
                table: "CheckInAttempts",
                column: "CheckInPassId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInAttempts_EventId_ScannedAt",
                table: "CheckInAttempts",
                columns: new[] { "EventId", "ScannedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckInPasses_EventId_Status",
                table: "CheckInPasses",
                columns: new[] { "EventId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckInPasses_QrToken",
                table: "CheckInPasses",
                column: "QrToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckInPasses_QrTokenHash",
                table: "CheckInPasses",
                column: "QrTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CheckInPasses_RegistrationTicketId",
                table: "CheckInPasses",
                column: "RegistrationTicketId",
                unique: true);

            migrationBuilder.Sql(
                """
                INSERT INTO "CheckInPasses" (
                    "Id",
                    "EventId",
                    "RegistrationOrderId",
                    "RegistrationTicketId",
                    "TicketTypeId",
                    "AttendeeName",
                    "AttendeeEmail",
                    "QrToken",
                    "QrTokenHash",
                    "Status",
                    "CreatedAt",
                    "CheckedInAt",
                    "CheckedInByUserId")
                SELECT
                    t."Id",
                    t."EventId",
                    t."OrderId",
                    t."Id",
                    t."TicketTypeId",
                    COALESCE(NULLIF(o."PersonalInfo_Name", ''), o."PersonalInfo_Email"),
                    o."PersonalInfo_Email",
                    t."QrToken",
                    t."QrTokenHash",
                    CASE
                        WHEN t."TicketState" = 2 THEN 3
                        WHEN t."CheckedInAt" IS NOT NULL THEN 2
                        ELSE 1
                    END,
                    t."CreatedAt",
                    t."CheckedInAt",
                    t."CheckedInByUserId"
                FROM "Tickets" t
                JOIN "Orders" o ON o."Id" = t."OrderId"
                WHERE t."QrToken" IS NOT NULL
                    AND t."QrTokenHash" IS NOT NULL
                """);

            migrationBuilder.DropColumn(
                name: "CheckedInAt",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CheckedInByUserId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "QrToken",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "QrTokenHash",
                table: "Tickets");
        }
    }
}

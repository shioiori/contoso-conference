using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eventbox.Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingPaymentUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cancel older duplicate pending payments, keeping only the latest per order
            migrationBuilder.Sql(
                """
                UPDATE "Payments"
                SET "Status" = 3
                WHERE "Status" = 0
                  AND "Id" NOT IN (
                      SELECT DISTINCT ON ("OrderId") "Id"
                      FROM "Payments"
                      WHERE "Status" = 0
                      ORDER BY "OrderId", "CreatedDate" DESC
                  );
                """);

            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX "IX_Payments_OrderId_Pending"
                ON "Payments" ("OrderId")
                WHERE "Status" = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_OrderId_Pending",
                table: "Payments");
        }
    }
}

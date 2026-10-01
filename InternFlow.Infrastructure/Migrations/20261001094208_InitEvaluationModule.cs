using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitEvaluationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 1, 9, 42, 7, 797, DateTimeKind.Utc).AddTicks(3914));

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 1, 9, 42, 7, 797, DateTimeKind.Utc).AddTicks(3914));

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 1, 9, 42, 7, 797, DateTimeKind.Utc).AddTicks(3914));

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 1, 9, 42, 7, 797, DateTimeKind.Utc).AddTicks(3914));

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000005"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 1, 9, 42, 7, 797, DateTimeKind.Utc).AddTicks(3914));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000001"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 1, 9, 38, 19, 355, DateTimeKind.Utc).AddTicks(416));

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000002"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 1, 9, 38, 19, 355, DateTimeKind.Utc).AddTicks(416));

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000003"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 1, 9, 38, 19, 355, DateTimeKind.Utc).AddTicks(416));

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000004"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 1, 9, 38, 19, 355, DateTimeKind.Utc).AddTicks(416));

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000005"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 1, 9, 38, 19, 355, DateTimeKind.Utc).AddTicks(416));
        }
    }
}

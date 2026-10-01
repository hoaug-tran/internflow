using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InternFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedEvaluationData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "dbo",
                table: "EvaluationPeriods",
                columns: new[] { "Id", "CreatedAt", "Description", "EndDate", "InternshipId", "Name", "StartDate", "Status", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("e1111111-1111-1111-1111-000000000001"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), null, new DateOnly(2026, 9, 11), new Guid("11111111-0000-0000-0000-000000000000"), "Đánh giá tháng 1", new DateOnly(2026, 9, 1), "Closed", null },
                    { new Guid("e1111111-1111-1111-1111-000000000002"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), null, new DateOnly(2026, 9, 26), new Guid("11111111-0000-0000-0000-000000000000"), "Đánh giá tháng 2", new DateOnly(2026, 9, 16), "Closed", null },
                    { new Guid("e1111111-1111-1111-1111-000000000003"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), null, new DateOnly(2026, 10, 11), new Guid("11111111-0000-0000-0000-000000000000"), "Đánh giá giữa kỳ", new DateOnly(2026, 10, 1), "Open", null },
                    { new Guid("e1111111-1111-1111-1111-000000000004"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), null, new DateOnly(2026, 10, 31), new Guid("11111111-0000-0000-0000-000000000000"), "Đánh giá tháng 4", new DateOnly(2026, 10, 21), "Open", null },
                    { new Guid("e1111111-1111-1111-1111-000000000005"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), null, new DateOnly(2026, 11, 20), new Guid("11111111-0000-0000-0000-000000000000"), "Đánh giá cuối kỳ", new DateOnly(2026, 11, 10), "Open", null }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Criteria",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayOrder", "EvaluationPeriodId", "IsActive", "Name", "UpdatedAt", "Weight" },
                values: new object[,]
                {
                    { new Guid("c1111111-1111-1111-1111-000000000001"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), null, 1, new Guid("e1111111-1111-1111-1111-000000000003"), true, "Thái độ làm việc và kỷ luật", null, 0.2m },
                    { new Guid("c1111111-1111-1111-1111-000000000002"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), null, 2, new Guid("e1111111-1111-1111-1111-000000000003"), true, "Kỹ năng lập trình", null, 0.3m },
                    { new Guid("c1111111-1111-1111-1111-000000000003"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), null, 3, new Guid("e1111111-1111-1111-1111-000000000003"), true, "Khả năng làm việc nhóm", null, 0.2m },
                    { new Guid("c1111111-1111-1111-1111-000000000004"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), null, 4, new Guid("e1111111-1111-1111-1111-000000000003"), true, "Tiến độ hoàn thành Task", null, 0.2m },
                    { new Guid("c1111111-1111-1111-1111-000000000005"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), null, 5, new Guid("e1111111-1111-1111-1111-000000000003"), true, "Khả năng chủ động học hỏi", null, 0.1m }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Evaluations",
                columns: new[] { "Id", "CreatedAt", "EvaluationPeriodId", "EvaluatorId", "InternId", "OverallComment", "Status", "SubmittedAt", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a1111111-1111-1111-1111-000000000001"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), new Guid("e1111111-1111-1111-1111-000000000003"), new Guid("22222222-0000-0000-0000-000000000000"), new Guid("33333333-0000-0000-0000-000000000001"), "Làm việc tốt, cần phát huy", "Finalized", new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402) },
                    { new Guid("a1111111-1111-1111-1111-000000000002"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), new Guid("e1111111-1111-1111-1111-000000000003"), new Guid("22222222-0000-0000-0000-000000000000"), new Guid("33333333-0000-0000-0000-000000000002"), "Đang trong quá trình chấm", "Draft", null, new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402) },
                    { new Guid("a1111111-1111-1111-1111-000000000003"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), new Guid("e1111111-1111-1111-1111-000000000003"), new Guid("22222222-0000-0000-0000-000000000000"), new Guid("33333333-0000-0000-0000-000000000003"), "Kỹ năng xuất sắc", "Finalized", new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402) },
                    { new Guid("a1111111-1111-1111-1111-000000000004"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), new Guid("e1111111-1111-1111-1111-000000000003"), new Guid("22222222-0000-0000-0000-000000000000"), new Guid("33333333-0000-0000-0000-000000000004"), "Thường xuyên trễ deadline", "Draft", null, new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402) },
                    { new Guid("a1111111-1111-1111-1111-000000000005"), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), new Guid("e1111111-1111-1111-1111-000000000003"), new Guid("22222222-0000-0000-0000-000000000000"), new Guid("33333333-0000-0000-0000-000000000005"), "Cần cải thiện giao tiếp", "Finalized", new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402), new DateTime(2026, 10, 1, 9, 20, 19, 80, DateTimeKind.Utc).AddTicks(2402) }
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "EvaluationDetails",
                columns: new[] { "Id", "Comment", "CriterionId", "EvaluationId", "Score" },
                values: new object[,]
                {
                    { new Guid("d1111111-1111-1111-1111-000000000001"), "Luôn đúng giờ", new Guid("c1111111-1111-1111-1111-000000000001"), new Guid("a1111111-1111-1111-1111-000000000001"), 9.0m },
                    { new Guid("d1111111-1111-1111-1111-000000000002"), "Code sạch, tối ưu tốt", new Guid("c1111111-1111-1111-1111-000000000002"), new Guid("a1111111-1111-1111-1111-000000000001"), 8.0m },
                    { new Guid("d1111111-1111-1111-1111-000000000003"), "Hỗ trợ đồng đội tốt", new Guid("c1111111-1111-1111-1111-000000000003"), new Guid("a1111111-1111-1111-1111-000000000001"), 8.5m },
                    { new Guid("d1111111-1111-1111-1111-000000000004"), "Hoàn thành trước hạn", new Guid("c1111111-1111-1111-1111-000000000004"), new Guid("a1111111-1111-1111-1111-000000000001"), 9.0m },
                    { new Guid("d1111111-1111-1111-1111-000000000005"), "Nên chủ động hỏi mentor nhiều hơn", new Guid("c1111111-1111-1111-1111-000000000005"), new Guid("a1111111-1111-1111-1111-000000000001"), 7.5m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "EvaluationDetails",
                keyColumn: "Id",
                keyValue: new Guid("d1111111-1111-1111-1111-000000000001"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "EvaluationDetails",
                keyColumn: "Id",
                keyValue: new Guid("d1111111-1111-1111-1111-000000000002"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "EvaluationDetails",
                keyColumn: "Id",
                keyValue: new Guid("d1111111-1111-1111-1111-000000000003"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "EvaluationDetails",
                keyColumn: "Id",
                keyValue: new Guid("d1111111-1111-1111-1111-000000000004"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "EvaluationDetails",
                keyColumn: "Id",
                keyValue: new Guid("d1111111-1111-1111-1111-000000000005"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "EvaluationPeriods",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-000000000001"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "EvaluationPeriods",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-000000000002"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "EvaluationPeriods",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-000000000004"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "EvaluationPeriods",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-000000000005"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Evaluations",
                keyColumn: "Id",
                keyValue: new Guid("a1111111-1111-1111-1111-000000000002"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Evaluations",
                keyColumn: "Id",
                keyValue: new Guid("a1111111-1111-1111-1111-000000000003"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Evaluations",
                keyColumn: "Id",
                keyValue: new Guid("a1111111-1111-1111-1111-000000000004"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Evaluations",
                keyColumn: "Id",
                keyValue: new Guid("a1111111-1111-1111-1111-000000000005"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000001"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000002"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000003"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000004"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Criteria",
                keyColumn: "Id",
                keyValue: new Guid("c1111111-1111-1111-1111-000000000005"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "Evaluations",
                keyColumn: "Id",
                keyValue: new Guid("a1111111-1111-1111-1111-000000000001"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "EvaluationPeriods",
                keyColumn: "Id",
                keyValue: new Guid("e1111111-1111-1111-1111-000000000003"));
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InternFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumnsToTasksAndWorkLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkLogs_Tasks_TaskItemId",
                schema: "dbo",
                table: "WorkLogs");

            migrationBuilder.DropIndex(
                name: "IX_WorkLogs_TaskItemId_WorkDate",
                schema: "dbo",
                table: "WorkLogs");

            migrationBuilder.DropIndex(
                name: "IX_WorkLogs_UserId",
                schema: "dbo",
                table: "WorkLogs");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ProjectId_Status",
                schema: "dbo",
                table: "Tasks");

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

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "dbo",
                table: "WorkLogs",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "dbo",
                table: "WorkLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "dbo",
                table: "WorkLogs",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                schema: "dbo",
                table: "Tasks",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "dbo",
                table: "Tasks",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateIndex(
                name: "IX_WorkLogs_TaskItemId",
                schema: "dbo",
                table: "WorkLogs",
                column: "TaskItemId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkLogs_UserId_WorkDate",
                schema: "dbo",
                table: "WorkLogs",
                columns: new[] { "UserId", "WorkDate" },
                filter: "[DeletedAt] IS NULL")
                .Annotation("SqlServer:Include", new[] { "TaskItemId", "Hours" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_WorkLogs_Description",
                schema: "dbo",
                table: "WorkLogs",
                sql: "LEN(LTRIM(RTRIM([Description]))) > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CreatedBy",
                schema: "dbo",
                table: "Tasks",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId_Status",
                schema: "dbo",
                table: "Tasks",
                columns: new[] { "ProjectId", "Status" })
                .Annotation("SqlServer:Include", new[] { "AssigneeId", "Priority", "DueDate" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Tasks_CompletedAt",
                schema: "dbo",
                table: "Tasks",
                sql: "([Status] = 'Done' AND [CompletedAt] IS NOT NULL) OR ([Status] <> 'Done' AND [CompletedAt] IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Tasks_Priority",
                schema: "dbo",
                table: "Tasks",
                sql: "[Priority] IN ('Low','Medium','High','Critical')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Tasks_Status",
                schema: "dbo",
                table: "Tasks",
                sql: "[Status] IN ('Todo','InProgress','InReview','Done','Cancelled')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Tasks_Title",
                schema: "dbo",
                table: "Tasks",
                sql: "LEN(LTRIM(RTRIM([Title]))) > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Users_CreatedBy",
                schema: "dbo",
                table: "Tasks",
                column: "CreatedBy",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkLogs_Tasks_TaskItemId",
                schema: "dbo",
                table: "WorkLogs",
                column: "TaskItemId",
                principalSchema: "dbo",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Users_CreatedBy",
                schema: "dbo",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkLogs_Tasks_TaskItemId",
                schema: "dbo",
                table: "WorkLogs");

            migrationBuilder.DropIndex(
                name: "IX_WorkLogs_TaskItemId",
                schema: "dbo",
                table: "WorkLogs");

            migrationBuilder.DropIndex(
                name: "IX_WorkLogs_UserId_WorkDate",
                schema: "dbo",
                table: "WorkLogs");

            migrationBuilder.DropCheckConstraint(
                name: "CK_WorkLogs_Description",
                schema: "dbo",
                table: "WorkLogs");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_CreatedBy",
                schema: "dbo",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ProjectId_Status",
                schema: "dbo",
                table: "Tasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Tasks_CompletedAt",
                schema: "dbo",
                table: "Tasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Tasks_Priority",
                schema: "dbo",
                table: "Tasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Tasks_Status",
                schema: "dbo",
                table: "Tasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Tasks_Title",
                schema: "dbo",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "dbo",
                table: "WorkLogs");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "dbo",
                table: "WorkLogs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "dbo",
                table: "WorkLogs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "dbo",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "dbo",
                table: "Tasks");

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "Criteria",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayOrder", "EvaluationPeriodId", "IsActive", "Name", "UpdatedAt", "Weight" },
                values: new object[,]
                {
                    { new Guid("c1111111-1111-1111-1111-000000000001"), new DateTime(2026, 10, 1, 9, 42, 7, 797, DateTimeKind.Utc).AddTicks(3914), null, 1, new Guid("e1111111-1111-1111-1111-000000000003"), true, "Thái độ làm việc và kỷ luật", null, 0.2m },
                    { new Guid("c1111111-1111-1111-1111-000000000002"), new DateTime(2026, 10, 1, 9, 42, 7, 797, DateTimeKind.Utc).AddTicks(3914), null, 2, new Guid("e1111111-1111-1111-1111-000000000003"), true, "Kỹ năng lập trình", null, 0.3m },
                    { new Guid("c1111111-1111-1111-1111-000000000003"), new DateTime(2026, 10, 1, 9, 42, 7, 797, DateTimeKind.Utc).AddTicks(3914), null, 3, new Guid("e1111111-1111-1111-1111-000000000003"), true, "Khả năng làm việc nhóm", null, 0.2m },
                    { new Guid("c1111111-1111-1111-1111-000000000004"), new DateTime(2026, 10, 1, 9, 42, 7, 797, DateTimeKind.Utc).AddTicks(3914), null, 4, new Guid("e1111111-1111-1111-1111-000000000003"), true, "Tiến độ hoàn thành Task", null, 0.2m },
                    { new Guid("c1111111-1111-1111-1111-000000000005"), new DateTime(2026, 10, 1, 9, 42, 7, 797, DateTimeKind.Utc).AddTicks(3914), null, 5, new Guid("e1111111-1111-1111-1111-000000000003"), true, "Khả năng chủ động học hỏi", null, 0.1m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkLogs_TaskItemId_WorkDate",
                schema: "dbo",
                table: "WorkLogs",
                columns: new[] { "TaskItemId", "WorkDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkLogs_UserId",
                schema: "dbo",
                table: "WorkLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId_Status",
                schema: "dbo",
                table: "Tasks",
                columns: new[] { "ProjectId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_WorkLogs_Tasks_TaskItemId",
                schema: "dbo",
                table: "WorkLogs",
                column: "TaskItemId",
                principalSchema: "dbo",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

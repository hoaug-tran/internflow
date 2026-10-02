using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRemovedAtAndRowVersionToProjectAndMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Internships_InternshipId",
                schema: "dbo",
                table: "Projects");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "dbo",
                table: "Projects",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemovedAt",
                schema: "dbo",
                table: "ProjectMembers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_InternshipId_Status",
                schema: "dbo",
                table: "Projects",
                columns: new[] { "InternshipId", "Status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Projects_Status",
                schema: "dbo",
                table: "Projects",
                sql: "[Status] IN ('Planned','Active','OnHold','Completed','Cancelled')");

            migrationBuilder.CreateIndex(
                name: "UX_ProjectMembers_Project_User_Active",
                schema: "dbo",
                table: "ProjectMembers",
                columns: new[] { "ProjectId", "UserId" },
                unique: true,
                filter: "[RemovedAt] IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Internships_InternshipId",
                schema: "dbo",
                table: "Projects",
                column: "InternshipId",
                principalSchema: "dbo",
                principalTable: "Internships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Internships_InternshipId",
                schema: "dbo",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_InternshipId_Status",
                schema: "dbo",
                table: "Projects");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Projects_Status",
                schema: "dbo",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "UX_ProjectMembers_Project_User_Active",
                schema: "dbo",
                table: "ProjectMembers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "dbo",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RemovedAt",
                schema: "dbo",
                table: "ProjectMembers");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Internships_InternshipId",
                schema: "dbo",
                table: "Projects",
                column: "InternshipId",
                principalSchema: "dbo",
                principalTable: "Internships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

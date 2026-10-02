using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentityAndInternship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Role = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FailedLoginCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LockoutEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.UniqueConstraint("UQ_Users_Id_Role", x => new { x.Id, x.Role });
                    table.CheckConstraint("CK_Users_Role", "[Role] IN ('Admin', 'Mentor', 'Intern')");
                });

            migrationBuilder.CreateTable(
                name: "InternProfiles",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserRole = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Intern"),
                    StudentCode = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    University = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Major = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    CvUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternProfiles", x => x.UserId);
                    table.CheckConstraint("CK_InternProfiles_Role", "[UserRole] = 'Intern'");
                    table.ForeignKey(
                        name: "FK_InternProfiles_Users",
                        columns: x => new { x.UserId, x.UserRole },
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumns: new[] { "Id", "Role" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Internships",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Planned"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Internships", x => x.Id);
                    table.CheckConstraint("CK_Batches_Dates", "[EndDate] > [StartDate]");
                    table.CheckConstraint("CK_Batches_Status", "[Status] IN ('Planned', 'Active', 'Closed')");
                    table.ForeignKey(
                        name: "FK_Batches_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MentorProfiles",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserRole = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Mentor"),
                    Department = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Expertise = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    MaxMentees = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorProfiles", x => x.UserId);
                    table.CheckConstraint("CK_MentorProfiles_Role", "[UserRole] = 'Mentor'");
                    table.ForeignKey(
                        name: "FK_MentorProfiles_Users",
                        columns: x => new { x.UserId, x.UserRole },
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumns: new[] { "Id", "Role" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    CreatedByIp = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByTokenId = table.Column<long>(type: "bigint", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RT_ReplacedBy",
                        column: x => x.ReplacedByTokenId,
                        principalSchema: "dbo",
                        principalTable: "RefreshTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RT_Users",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BatchInterns",
                schema: "dbo",
                columns: table => new
                {
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InternId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InternRole = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Intern"),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "InProgress"),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    AddedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchInterns", x => new { x.BatchId, x.InternId });
                    table.CheckConstraint("CK_BI_Role", "[InternRole] = 'Intern'");
                    table.CheckConstraint("CK_BI_Status", "[Status] IN ('InProgress', 'Completed', 'DroppedOut', 'Failed')");
                    table.ForeignKey(
                        name: "FK_BI_AddedBy",
                        column: x => x.AddedBy,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BI_Batch",
                        column: x => x.BatchId,
                        principalSchema: "dbo",
                        principalTable: "Internships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BI_Intern",
                        columns: x => new { x.InternId, x.InternRole },
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumns: new[] { "Id", "Role" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BatchMentors",
                schema: "dbo",
                columns: table => new
                {
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorRole = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "Mentor"),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    AddedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchMentors", x => new { x.BatchId, x.MentorId });
                    table.CheckConstraint("CK_BM_Role", "[MentorRole] = 'Mentor'");
                    table.ForeignKey(
                        name: "FK_BM_AddedBy",
                        column: x => x.AddedBy,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BM_Batch",
                        column: x => x.BatchId,
                        principalSchema: "dbo",
                        principalTable: "Internships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BM_Mentor",
                        columns: x => new { x.MentorId, x.MentorRole },
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumns: new[] { "Id", "Role" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MentorAssignments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InternId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MentorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    AssignedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false, computedColumnSql: "CASE WHEN [EndedAt] IS NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorAssignments", x => x.Id);
                    table.CheckConstraint("CK_MA_Ended", "[EndedAt] IS NULL OR [EndedAt] >= [AssignedAt]");
                    table.ForeignKey(
                        name: "FK_MA_AssignedBy",
                        column: x => x.AssignedBy,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MA_BatchIntern",
                        columns: x => new { x.BatchId, x.InternId },
                        principalSchema: "dbo",
                        principalTable: "BatchInterns",
                        principalColumns: new[] { "BatchId", "InternId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MA_BatchMentor",
                        columns: x => new { x.BatchId, x.MentorId },
                        principalSchema: "dbo",
                        principalTable: "BatchMentors",
                        principalColumns: new[] { "BatchId", "MentorId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MentorAssignments_Internships_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "dbo",
                        principalTable: "Internships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BatchInterns_AddedBy",
                schema: "dbo",
                table: "BatchInterns",
                column: "AddedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BatchInterns_InternId_InternRole",
                schema: "dbo",
                table: "BatchInterns",
                columns: new[] { "InternId", "InternRole" });

            migrationBuilder.CreateIndex(
                name: "IX_BI_Intern",
                schema: "dbo",
                table: "BatchInterns",
                column: "InternId");

            migrationBuilder.CreateIndex(
                name: "IX_BI_Status",
                schema: "dbo",
                table: "BatchInterns",
                columns: new[] { "BatchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BatchMentors_AddedBy",
                schema: "dbo",
                table: "BatchMentors",
                column: "AddedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BatchMentors_MentorId_MentorRole",
                schema: "dbo",
                table: "BatchMentors",
                columns: new[] { "MentorId", "MentorRole" });

            migrationBuilder.CreateIndex(
                name: "IX_BM_Mentor",
                schema: "dbo",
                table: "BatchMentors",
                column: "MentorId");

            migrationBuilder.CreateIndex(
                name: "IX_InternProfiles_UserId_UserRole",
                schema: "dbo",
                table: "InternProfiles",
                columns: new[] { "UserId", "UserRole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_InternProfiles_StudentCode",
                schema: "dbo",
                table: "InternProfiles",
                column: "StudentCode",
                unique: true,
                filter: "[StudentCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_Status",
                schema: "dbo",
                table: "Internships",
                columns: new[] { "Status", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Internships_CreatedBy",
                schema: "dbo",
                table: "Internships",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "UQ_Batches_Name",
                schema: "dbo",
                table: "Internships",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MA_CurrentByMentor",
                schema: "dbo",
                table: "MentorAssignments",
                columns: new[] { "MentorId", "BatchId" },
                filter: "[EndedAt] IS NULL")
                .Annotation("SqlServer:Include", new[] { "InternId" });

            migrationBuilder.CreateIndex(
                name: "IX_MA_HistoryByIntern",
                schema: "dbo",
                table: "MentorAssignments",
                columns: new[] { "BatchId", "InternId", "AssignedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_MentorAssignments_AssignedBy",
                schema: "dbo",
                table: "MentorAssignments",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MentorAssignments_BatchId_MentorId",
                schema: "dbo",
                table: "MentorAssignments",
                columns: new[] { "BatchId", "MentorId" });

            migrationBuilder.CreateIndex(
                name: "UX_MA_OneCurrentMentor",
                schema: "dbo",
                table: "MentorAssignments",
                columns: new[] { "BatchId", "InternId" },
                unique: true,
                filter: "[EndedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MentorProfiles_UserId_UserRole",
                schema: "dbo",
                table: "MentorProfiles",
                columns: new[] { "UserId", "UserRole" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Family",
                schema: "dbo",
                table: "RefreshTokens",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Purge",
                schema: "dbo",
                table: "RefreshTokens",
                column: "ExpiresAt")
                .Annotation("SqlServer:Include", new[] { "RevokedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ReplacedByTokenId",
                schema: "dbo",
                table: "RefreshTokens",
                column: "ReplacedByTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                schema: "dbo",
                table: "RefreshTokens",
                column: "UserId")
                .Annotation("SqlServer:Include", new[] { "FamilyId", "ExpiresAt", "RevokedAt" });

            migrationBuilder.CreateIndex(
                name: "UQ_RefreshTokens_Hash",
                schema: "dbo",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Users_Email",
                schema: "dbo",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InternProfiles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MentorAssignments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MentorProfiles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "RefreshTokens",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BatchInterns",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "BatchMentors",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Internships",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "dbo");
        }
    }
}

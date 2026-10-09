using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkloadReportProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
CREATE OR ALTER PROCEDURE dbo.usp_GetInternWorkloadReport
    @RequesterId UNIQUEIDENTIFIER,
    @IsAdmin     BIT,
    @ProjectId   UNIQUEIDENTIFIER = NULL,
    @FromDate    DATE = NULL,
    @ToDate      DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH ScopedInterns AS
    (
        -- Admin: mọi intern. Mentor: chỉ intern đang được mình phụ trách (EndedAt IS NULL)
        SELECT bi.InternId, bi.BatchId
        FROM dbo.BatchInterns bi
        WHERE (@ProjectId IS NULL
               OR bi.BatchId = (SELECT p.InternshipId FROM dbo.Projects p WHERE p.Id = @ProjectId))
          AND (@IsAdmin = 1
               OR EXISTS (SELECT 1
                          FROM dbo.MentorAssignments ma
                          WHERE ma.BatchId  = bi.BatchId
                            AND ma.InternId = bi.InternId
                            AND ma.MentorId = @RequesterId
                            AND ma.EndedAt IS NULL))
    ),
    LogScope AS
    (
        SELECT w.UserId, w.TaskItemId, w.WorkDate, w.Hours, p.InternshipId AS BatchId
        FROM dbo.WorkLogs w
        INNER JOIN dbo.Tasks t    ON t.Id = w.TaskItemId
        INNER JOIN dbo.Projects p ON p.Id = t.ProjectId
        WHERE w.DeletedAt IS NULL
          AND (@ProjectId IS NULL OR t.ProjectId = @ProjectId)
          AND (@FromDate  IS NULL OR w.WorkDate >= @FromDate)
          AND (@ToDate    IS NULL OR w.WorkDate <= @ToDate)
    )
    SELECT
        u.Id       AS InternId,
        u.FullName AS FullName,
        CAST(ISNULL(SUM(ls.Hours), 0) AS DECIMAL(10, 2)) AS TotalHours,
        COUNT(ls.TaskItemId)             AS WorkLogCount,
        COUNT(DISTINCT ls.WorkDate)      AS WorkDays,
        COUNT(DISTINCT ls.TaskItemId)    AS TaskCount,
        CAST(CASE WHEN COUNT(DISTINCT ls.WorkDate) = 0 THEN 0
                  ELSE SUM(ls.Hours) / COUNT(DISTINCT ls.WorkDate) END AS DECIMAL(10, 2)) AS AvgHoursPerWorkDay
    FROM ScopedInterns si
    INNER JOIN dbo.Users u ON u.Id = si.InternId
    LEFT JOIN LogScope ls  ON ls.UserId = si.InternId AND ls.BatchId = si.BatchId
    GROUP BY u.Id, u.FullName
    ORDER BY u.FullName;
END
""");

            migrationBuilder.Sql("""
CREATE OR ALTER PROCEDURE dbo.usp_GetMentorWorkloadReport
    @RequesterId UNIQUEIDENTIFIER,
    @IsAdmin     BIT,
    @ProjectId   UNIQUEIDENTIFIER = NULL,
    @FromDate    DATE = NULL,
    @ToDate      DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH ScopedMentors AS
    (
        -- Admin: mọi mentor. Mentor: chỉ dòng của chính mình
        SELECT DISTINCT bm.MentorId
        FROM dbo.BatchMentors bm
        WHERE (@ProjectId IS NULL
               OR bm.BatchId = (SELECT p.InternshipId FROM dbo.Projects p WHERE p.Id = @ProjectId))
          AND (@IsAdmin = 1 OR bm.MentorId = @RequesterId)
    ),
    CurrentAssignments AS
    (
        SELECT ma.MentorId, ma.InternId, ma.BatchId
        FROM dbo.MentorAssignments ma
        WHERE ma.EndedAt IS NULL
          AND (@ProjectId IS NULL
               OR ma.BatchId = (SELECT p.InternshipId FROM dbo.Projects p WHERE p.Id = @ProjectId))
    ),
    LogByIntern AS
    (
        -- Gộp sẵn theo (intern, đợt) để join không bị nhân đôi dòng
        SELECT w.UserId, p.InternshipId AS BatchId, SUM(w.Hours) AS Hours
        FROM dbo.WorkLogs w
        INNER JOIN dbo.Tasks t    ON t.Id = w.TaskItemId
        INNER JOIN dbo.Projects p ON p.Id = t.ProjectId
        WHERE w.DeletedAt IS NULL
          AND (@ProjectId IS NULL OR t.ProjectId = @ProjectId)
          AND (@FromDate  IS NULL OR w.WorkDate >= @FromDate)
          AND (@ToDate    IS NULL OR w.WorkDate <= @ToDate)
        GROUP BY w.UserId, p.InternshipId
    )
    SELECT
        u.Id       AS MentorId,
        u.FullName AS FullName,
        COUNT(DISTINCT ca.InternId) AS InternCount,
        CAST(ISNULL(SUM(l.Hours), 0) AS DECIMAL(10, 2)) AS TotalHours,
    CAST(CASE WHEN COUNT(DISTINCT ca.InternId) = 0 THEN 0
          ELSE ISNULL(SUM(l.Hours), 0) / COUNT(DISTINCT ca.InternId) END AS DECIMAL(10, 2)) AS AvgHoursPerIntern
    FROM ScopedMentors sm
    INNER JOIN dbo.Users u ON u.Id = sm.MentorId
    LEFT JOIN CurrentAssignments ca ON ca.MentorId = sm.MentorId
    LEFT JOIN LogByIntern l ON l.UserId = ca.InternId AND l.BatchId = ca.BatchId
    GROUP BY u.Id, u.FullName
    ORDER BY u.FullName;
END
""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.usp_GetMentorWorkloadReport;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.usp_GetInternWorkloadReport;");
        }
    }
}

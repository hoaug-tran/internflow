using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskProgressReportProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
CREATE OR ALTER PROCEDURE dbo.usp_GetTaskProgressReport
    @RequesterId UNIQUEIDENTIFIER,
    @IsAdmin     BIT,
    @ProjectId   UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);

  ;WITH ScopedInterns AS
(
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
    TaskScope AS
    (
        SELECT t.Id, t.AssigneeId, t.Status, t.DueDate, p.InternshipId AS BatchId
        FROM dbo.Tasks t
        INNER JOIN dbo.Projects p ON p.Id = t.ProjectId
        WHERE t.AssigneeId IS NOT NULL
          AND (@ProjectId IS NULL OR t.ProjectId = @ProjectId)
    )
    SELECT
        u.Id       AS InternId,
        u.FullName AS FullName,
        COUNT(ts.Id) AS TotalTasks,
        SUM(CASE WHEN ts.Status = 'Todo'       THEN 1 ELSE 0 END) AS Todo,
        SUM(CASE WHEN ts.Status = 'InProgress' THEN 1 ELSE 0 END) AS InProgress,
        SUM(CASE WHEN ts.Status = 'InReview'   THEN 1 ELSE 0 END) AS InReview,
        SUM(CASE WHEN ts.Status = 'Done'       THEN 1 ELSE 0 END) AS Done,
        SUM(CASE WHEN ts.Status = 'Cancelled'  THEN 1 ELSE 0 END) AS Cancelled,
        SUM(CASE WHEN ts.DueDate < @Today
                  AND ts.Status NOT IN ('Done', 'Cancelled') THEN 1 ELSE 0 END) AS Overdue,
        CAST(CASE
                WHEN COUNT(ts.Id) - SUM(CASE WHEN ts.Status = 'Cancelled' THEN 1 ELSE 0 END) = 0
                    THEN 0
                ELSE 100.0 * SUM(CASE WHEN ts.Status = 'Done' THEN 1 ELSE 0 END)
                     / (COUNT(ts.Id) - SUM(CASE WHEN ts.Status = 'Cancelled' THEN 1 ELSE 0 END))
             END AS DECIMAL(5, 2)) AS CompletionRate,
        CAST(SUM(ISNULL(wl.Hours, 0)) AS DECIMAL(10, 2)) AS TotalHours
    FROM ScopedInterns si
    INNER JOIN dbo.Users u ON u.Id = si.InternId
    LEFT JOIN TaskScope ts ON ts.AssigneeId = si.InternId AND ts.BatchId = si.BatchId
    OUTER APPLY
    (
        -- Mỗi task chỉ ra 1 dòng nên không bị nhân đôi khi join
        SELECT SUM(w.Hours) AS Hours
        FROM dbo.WorkLogs w
        WHERE w.TaskItemId = ts.Id
          AND w.UserId = ts.AssigneeId
          AND w.DeletedAt IS NULL
    ) wl
    GROUP BY u.Id, u.FullName
    ORDER BY u.FullName;
END
""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.usp_GetTaskProgressReport;");
        }
    }
}

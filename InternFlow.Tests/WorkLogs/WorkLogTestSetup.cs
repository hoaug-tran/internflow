using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;
using InternFlow.Domain.Projects;
using InternFlow.Infrastructure.Data;

using TaskStatus = InternFlow.Domain.Projects.TaskStatus;

namespace InternFlow.Tests.Common.TestHelpers;

/// <summary>
/// Helper seed data dùng chung cho mọi WorkLog test.
/// </summary>
internal static class WorkLogTestSetup
{
    /// <summary>
    /// Seed Internship (Active, bao trùm hôm nay) + Mentor + Intern (thuộc batch) + Project + Task.
    /// Task mặc định InProgress — hợp lệ để ghi worklog.
    /// Trả về (internshipId, mentorId, internId, projectId, taskId).
    /// </summary>
    public static async Task<(Guid InternshipId, Guid MentorId, Guid InternId, Guid ProjectId, Guid TaskId)>
        SetupTaskForWorkLogAsync(
            InternFlowDbContext db,
            TaskStatus taskStatus = TaskStatus.InProgress)
    {
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var mentorId = Guid.NewGuid();
        var internId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Internship bao trùm hôm nay
        db.Internships.Add(new Internship
        {
            Id = internshipId,
            Name = "Test Internship",
            StartDate = today.AddDays(-30),
            EndDate = today.AddDays(30),
            Status = InternshipStatus.Active,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow
        });

        db.Users.AddRange(
            EntityFactory.CreateUser(mentorId, UserRole.Mentor),
            EntityFactory.CreateUser(internId, UserRole.Intern));

        db.BatchInterns.Add(new BatchIntern
        {
            InternshipId = internshipId,
            InternId = internId,
            InternRole = UserRole.Intern,
            Status = BatchInternStatus.InProgress,
            AddedBy = adminId,
            JoinedAt = DateTime.UtcNow
        });

        var project = EntityFactory.CreateProject(internshipId, mentorId);
        project.Id = projectId;
        project.Status = ProjectStatus.Active;
        db.Projects.Add(project);

        db.ProjectMembers.AddRange(
            new ProjectMember
            {
                ProjectId = projectId,
                UserId = mentorId,
                Role = MemberRole.Lead,
                JoinedAt = DateTime.UtcNow
            },
            new ProjectMember
            {
                ProjectId = projectId,
                UserId = internId,
                Role = MemberRole.Member,
                JoinedAt = DateTime.UtcNow
            });

        db.Tasks.Add(new TaskItem
        {
            Id = taskId,
            ProjectId = projectId,
            AssigneeId = internId,
            Title = "Task for worklog",
            Description = "Test",
            Status = taskStatus,
            Priority = TaskPriority.Medium,
            CompletedAt = taskStatus == TaskStatus.Done ? DateTime.UtcNow : null,
            CreatedBy = mentorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return (internshipId, mentorId, internId, projectId, taskId);
    }

    /// <summary>
    /// Seed 1 worklog trực tiếp vào DB.
    /// </summary>
    public static async Task<Guid> SeedWorkLogAsync(
        InternFlowDbContext db,
        Guid taskId,
        Guid userId,
        DateOnly? workDate = null,
        decimal hours = 4,
        string description = "Seeded worklog",
        DateTime? createdAt = null)
    {
        var workLogId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        db.WorkLogs.Add(new WorkLog
        {
            Id = workLogId,
            TaskItemId = taskId,
            UserId = userId,
            WorkDate = workDate ?? DateOnly.FromDateTime(now),
            Hours = hours,
            Description = description,
            CreatedAt = createdAt ?? now,
            UpdatedAt = now
        });
        await db.SaveChangesAsync();
        return workLogId;
    }
}

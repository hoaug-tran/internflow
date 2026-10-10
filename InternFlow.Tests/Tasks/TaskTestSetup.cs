using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Infrastructure.Data;
using InternFlow.Tests.Common.TestHelpers;
using TaskStatus = InternFlow.Domain.Projects.TaskStatus;

namespace InternFlow.Tests.Tasks;

/// <summary>
/// Helper seed data dùng chung cho mọi Task test
/// (Create / ChangeStatus / Update / GetTasks).
/// </summary>
internal static class TaskTestSetup
{
    /// <summary>
    /// Seed Internship + Mentor (Lead) + Intern (Member) + Project.
    /// Trả về (internshipId, mentorId, internId, projectId).
    /// </summary>
    public static async Task<(Guid InternshipId, Guid MentorId, Guid InternId, Guid ProjectId)>
        SetupProjectWithInternAsync(InternFlowDbContext db)
    {
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var mentorId = Guid.NewGuid();
        var internId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));
        db.Users.AddRange(
            EntityFactory.CreateUser(mentorId, UserRole.Mentor),
            EntityFactory.CreateUser(internId, UserRole.Intern));

        var project = EntityFactory.CreateProject(internshipId, mentorId);
        project.Id = projectId;
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

        await db.SaveChangesAsync();
        return (internshipId, mentorId, internId, projectId);
    }

    /// <summary>
    /// Seed 1 task trực tiếp vào DB.
    /// </summary>
    public static async Task<Guid> SeedTaskAsync(
        InternFlowDbContext db,
        Guid projectId,
        Guid assigneeId,
        Guid createdBy,
        TaskStatus status = TaskStatus.Todo,
        TaskPriority priority = TaskPriority.Medium,
        string title = "Test Task",
        DateOnly? dueDate = null)
    {
        var taskId = Guid.NewGuid();
        db.Tasks.Add(new TaskItem
        {
            Id = taskId,
            ProjectId = projectId,
            AssigneeId = assigneeId,
            CreatedBy = createdBy,
            Title = title,
            Description = "Description",
            Status = status,
            Priority = priority,
            DueDate = dueDate,
            CompletedAt = status == TaskStatus.Done ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return taskId;
    }
}

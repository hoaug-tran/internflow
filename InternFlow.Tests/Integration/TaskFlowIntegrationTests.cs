using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;
using InternFlow.Domain.Projects;
using InternFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using TaskStatus = InternFlow.Domain.Projects.TaskStatus;

namespace InternFlow.Tests.Integration;

[Collection("Integration")]
public class TaskFlowIntegrationTests : IntegrationTestBase
{
    public TaskFlowIntegrationTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task CreateTask_ValidRequest_PersistsToDb()
    {
        await AuthenticateAsync();

        var (projectId, internId, _) = await SeedProjectWithInternAsync();

        var response = await Client.PostAsJsonAsync(
            $"/api/projects/{projectId}/tasks",
            new
            {
                title = "Integration Task",
                description = "Created via HTTP",
                priority = "High",
                dueDate = "2026-12-31",
                assigneeId = internId
            });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();

        var task = await db.Tasks
            .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Title == "Integration Task");

        task.Should().NotBeNull();
        task!.Status.Should().Be(TaskStatus.Todo);
        task.Priority.Should().Be(TaskPriority.High);
        task.AssigneeId.Should().Be(internId);
    }

    [Fact]
    public async Task ChangeTaskStatus_ValidTransition_UpdatesDb()
    {
        await AuthenticateAsync();
        var (projectId, internId, _) = await SeedProjectWithInternAsync();
        var taskId = await SeedTaskAsync(projectId, internId, TaskStatus.Todo);

        var response = await Client.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/status",
            new { newStatus = "InProgress" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();
        var task = await db.Tasks.FindAsync(taskId);

        task!.Status.Should().Be(TaskStatus.InProgress);
    }

    [Fact]
    public async Task ChangeTaskStatus_InvalidTransition_Returns409()
    {
        await AuthenticateAsync();
        var (projectId, internId, _) = await SeedProjectWithInternAsync();
        var taskId = await SeedTaskAsync(projectId, internId, TaskStatus.Todo);

        var response = await Client.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/status",
            new { newStatus = "Done" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetTasks_ReturnsPagedResult()
    {
        await AuthenticateAsync();
        var (projectId, internId, _) = await SeedProjectWithInternAsync();

        await SeedTaskAsync(projectId, internId, TaskStatus.Todo);
        await SeedTaskAsync(projectId, internId, TaskStatus.InProgress);

        var response = await Client.GetAsync($"/api/tasks?projectId={projectId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTaskById_ValidId_ReturnsTask()
    {
        await AuthenticateAsync();
        var (projectId, internId, _) = await SeedProjectWithInternAsync();
        var taskId = await SeedTaskAsync(projectId, internId, TaskStatus.Todo);

        var response = await Client.GetAsync($"/api/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateTask_ValidRequest_UpdatesDb()
    {
        await AuthenticateAsync();
        var (projectId, internId, _) = await SeedProjectWithInternAsync();
        var taskId = await SeedTaskAsync(projectId, internId, TaskStatus.Todo);

        var response = await Client.PatchAsJsonAsync($"/api/tasks/{taskId}", new
        {
            title = "Updated Title",
            description = "Updated",
            priority = "Critical",
            dueDate = "2026-12-31",
            assigneeId = internId
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();
        var task = await db.Tasks.FindAsync(taskId);

        task!.Title.Should().Be("Updated Title");
        task.Priority.Should().Be(TaskPriority.Critical);
    }

    // ═══════════════════════════════════════════════════
    // Seeding helpers
    // ═══════════════════════════════════════════════════

    private async Task<(Guid ProjectId, Guid InternId, Guid MentorId)> SeedProjectWithInternAsync()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();

        // ★ Dùng lại Admin + Internship từ DevSeeder (đã tồn tại)
        var adminId = DevSeeder.AdminId;
        var internshipId = DevSeeder.InternshipId;

        // Mentor + Intern mới cho project này
        var mentorId = Guid.NewGuid();
        var internId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        // ★ Bước 1: thêm Users trước (chưa tham chiếu gì)
        db.Users.AddRange(
            new User
            {
                Id = mentorId,
                Email = $"mentor-{mentorId:N}@test.com",
                FullName = "Mentor Test",
                PasswordHash = "hash",
                Role = UserRole.Mentor,
                IsActive = true,
                RowVersion = Array.Empty<byte>(),
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = internId,
                Email = $"intern-{internId:N}@test.com",
                FullName = "Intern Test",
                PasswordHash = "hash",
                Role = UserRole.Intern,
                IsActive = true,
                RowVersion = Array.Empty<byte>(),
                CreatedAt = DateTime.UtcNow
            });

        // ★ Lưu Users trước để FK CreatedBy/AddedBy resolve được
        await db.SaveChangesAsync();

        // ★ Bước 2: Project trỏ tới Internship có sẵn + Mentor vừa tạo
        db.Projects.Add(new Project
        {
            Id = projectId,
            InternshipId = internshipId,      // ← DevSeeder.InternshipId
            Name = $"Integration Project {Guid.NewGuid():N}",
            Status = ProjectStatus.Active,
            CreatedBy = mentorId,             // ← vừa tạo, đã lưu
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

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

        // ★ BatchIntern dùng Internship có sẵn + AddedBy = Admin có sẵn
        db.BatchInterns.Add(new BatchIntern
        {
            InternshipId = internshipId,      // ← DevSeeder.InternshipId
            InternId = internId,
            InternRole = UserRole.Intern,
            Status = BatchInternStatus.InProgress,
            AddedBy = adminId,                // ← DevSeeder.AdminId, tồn tại
            JoinedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return (projectId, internId, mentorId);
    }

    private async Task<Guid> SeedTaskAsync(Guid projectId, Guid assigneeId, TaskStatus status)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();

        var taskId = Guid.NewGuid();
        db.Tasks.Add(new TaskItem
        {
            Id = taskId,
            ProjectId = projectId,
            AssigneeId = assigneeId,
            Title = $"Task {Guid.NewGuid():N}",
            Description = "Seeded",
            Status = status,
            Priority = TaskPriority.Medium,
            CreatedBy = DevSeeder.MentorId,   // ★ dùng Mentor có sẵn thay vì assigneeId
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return taskId;
    }
}
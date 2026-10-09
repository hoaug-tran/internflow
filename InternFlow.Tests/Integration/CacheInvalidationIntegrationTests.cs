using System.Net.Http.Json;
using FluentAssertions;
using InternFlow.Application.Common.Caching;
using InternFlow.Application.Features.Tasks.Queries.GetTaskSummary;
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
public class CacheInvalidationIntegrationTests : IntegrationTestBase
{
    private readonly FakeCacheService _cache;

    public CacheInvalidationIntegrationTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
        _cache = factory.Services.GetRequiredService<FakeCacheService>();
        _cache.Clear();
    }

    [Fact]
    public async Task GetTaskSummary_PopulatesCache()
    {
        await AuthenticateAsync();
        var (projectId, internId, _) = await SeedProjectWithTaskAsync();

        var response = await Client.GetAsync(
            $"/api/projects/{projectId}/tasks/summary");

        response.EnsureSuccessStatusCode();

        var expectedKey = CacheKeys.ForProject<GetTaskSummaryQuery>(projectId);
        _cache.HasKey(expectedKey).Should().BeTrue(
            $"Expected cache key {expectedKey} to be populated");
    }

    [Fact]
    public async Task ChangeTaskStatus_InvalidatesTaskSummaryCache()
    {
        await AuthenticateAsync();
        var (projectId, internId, _) = await SeedProjectWithTaskAsync();
        var taskId = await GetFirstTaskIdAsync(projectId);

        await Client.GetAsync($"/api/projects/{projectId}/tasks/summary");

        var cacheKey = CacheKeys.ForProject<GetTaskSummaryQuery>(projectId);
        _cache.HasKey(cacheKey).Should().BeTrue("Cache should be warmed");

        var response = await Client.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/status",
            new { newStatus = "InProgress" });

        response.EnsureSuccessStatusCode();

        _cache.HasKey(cacheKey).Should().BeFalse(
            "Cache should be invalidated after task status change");
    }

    [Fact]
    public async Task CreateTask_InvalidatesTaskSummaryCache()
    {
        await AuthenticateAsync();
        var (projectId, internId, _) = await SeedProjectWithTaskAsync();

        await Client.GetAsync($"/api/projects/{projectId}/tasks/summary");
        var cacheKey = CacheKeys.ForProject<GetTaskSummaryQuery>(projectId);
        _cache.HasKey(cacheKey).Should().BeTrue();

        var response = await Client.PostAsJsonAsync(
            $"/api/projects/{projectId}/tasks",
            new
            {
                title = "New Task",
                description = "X",
                priority = "Medium",
                dueDate = "2026-12-31",
                assigneeId = internId
            });

        response.EnsureSuccessStatusCode();

        _cache.HasKey(cacheKey).Should().BeFalse();
    }

    [Fact]
    public async Task SecondCall_HitsCache()
    {
        await AuthenticateAsync();
        var (projectId, _, _) = await SeedProjectWithTaskAsync();

        var r1 = await Client.GetAsync($"/api/projects/{projectId}/tasks/summary");
        var r2 = await Client.GetAsync($"/api/projects/{projectId}/tasks/summary");

        r1.EnsureSuccessStatusCode();
        r2.EnsureSuccessStatusCode();

        var cacheKey = CacheKeys.ForProject<GetTaskSummaryQuery>(projectId);
        _cache.HasKey(cacheKey).Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════
    // Seeding helpers
    // ═══════════════════════════════════════════════════

    private async Task<(Guid ProjectId, Guid InternId, Guid MentorId)>
        SeedProjectWithTaskAsync()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();

        // ★ Dùng lại Admin + Internship từ DevSeeder (đã tồn tại trong DB)
        var adminId = DevSeeder.AdminId;
        var internshipId = DevSeeder.InternshipId;

        // Mentor + Intern mới cho project này
        var mentorId = Guid.NewGuid();
        var internId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        // ★ Bước 1: thêm Users trước (chưa tham chiếu gì)
        db.Users.AddRange(
            new User
            {
                Id = mentorId,
                Email = $"m-{mentorId:N}@t.com",
                FullName = "Mentor",
                PasswordHash = "hash",
                Role = UserRole.Mentor,
                IsActive = true,
                RowVersion = Array.Empty<byte>(),
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = internId,
                Email = $"i-{internId:N}@t.com",
                FullName = "Intern",
                PasswordHash = "hash",
                Role = UserRole.Intern,
                IsActive = true,
                RowVersion = Array.Empty<byte>(),
                CreatedAt = DateTime.UtcNow
            });

        // ★ Lưu Users trước để FK CreatedBy/AssigneeId resolve được
        await db.SaveChangesAsync();

        // ★ Bước 2: Project trỏ tới Internship có sẵn + Mentor vừa tạo
        db.Projects.Add(new Project
        {
            Id = projectId,
            InternshipId = internshipId,      // ← DevSeeder.InternshipId
            Name = $"P {Guid.NewGuid():N}",
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

        db.Tasks.Add(new TaskItem
        {
            Id = taskId,
            ProjectId = projectId,
            AssigneeId = internId,
            Title = "Seeded Task",
            Description = "For cache test",
            Status = TaskStatus.Todo,
            Priority = TaskPriority.Medium,
            CreatedBy = mentorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
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

    private async Task<Guid> GetFirstTaskIdAsync(Guid projectId)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();

        return await db.Tasks
            .Where(t => t.ProjectId == projectId)
            .Select(t => t.Id)
            .FirstAsync();
    }
}
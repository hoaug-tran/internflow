using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;
using InternFlow.Domain.Projects;
using InternFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InternFlow.Tests.Integration;

[Collection("Integration")]
public class ProjectFlowIntegrationTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ProjectFlowIntegrationTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task CreateProject_ValidRequest_PersistsToDb()
    {
        // ─── Arrange ───
        await AuthenticateAsync();

        // ★ Dùng Admin có thật từ DevSeeder
        var adminId = DevSeeder.AdminId;
        var internshipId = Guid.NewGuid();
        await SeedInternshipAsync(internshipId, adminId);

        var projectName = $"Integration Test {Guid.NewGuid():N}";

        // ─── Act ───
        var response = await Client.PostAsJsonAsync("/api/projects", new
        {
            internshipId,
            name = projectName,
            description = "Created via integration test",
            startDate = "2026-10-01",
            endDate = "2026-12-31"
        });

        // ─── Assert HTTP ───
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<ProjectResponseDto>>(JsonOptions);
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Name.Should().Be(projectName);

        // ─── Assert DB ───
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();

        var projectInDb = await db.Projects
            .FirstOrDefaultAsync(p => p.Name == projectName);

        projectInDb.Should().NotBeNull();
        projectInDb!.InternshipId.Should().Be(internshipId);

        // Người tạo tự động thành Lead
        var lead = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectInDb.Id);

        lead.Should().NotBeNull();
        lead!.Role.Should().Be(MemberRole.Lead);
    }

    [Fact]
    public async Task GetProject_ValidId_ReturnsProject()
    {
        // ─── Arrange ───
        await AuthenticateAsync();

        var adminId = DevSeeder.AdminId;
        var internshipId = Guid.NewGuid();
        var projectId = await SeedProjectAsync(internshipId, adminId, "Get Project Test");

        // ─── Act ───
        var response = await Client.GetAsync($"/api/projects/{projectId}");

        // ─── Assert ───
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProjects_ReturnsListFilteredByScope()
    {
        // ─── Arrange ───
        await AuthenticateAsync();

        var adminId = DevSeeder.AdminId;
        var internshipId = Guid.NewGuid();
        await SeedInternshipAsync(internshipId, adminId);
        await SeedProjectAsync(internshipId, adminId, "Project 1");
        await SeedProjectAsync(internshipId, adminId, "Project 2");

        // ─── Act ───
        var response = await Client.GetAsync("/api/projects");

        // ─── Assert ───
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content
            .ReadFromJsonAsync<ApiEnvelope<PagedEnvelope<ProjectListItemDto>>>(JsonOptions);
        body!.Data.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task UpdateProject_ValidRequest_UpdatesDb()
    {
        // ─── Arrange ───
        await AuthenticateAsync();

        var adminId = DevSeeder.AdminId;
        var internshipId = Guid.NewGuid();
        var projectId = await SeedProjectAsync(internshipId, adminId, "Old Name");

        var newName = $"Updated {Guid.NewGuid():N}";

        // ─── Act ───
        var response = await Client.PutAsJsonAsync($"/api/projects/{projectId}", new
        {
            name = newName,
            description = "Updated",
            status = "Active",
            startDate = "2026-10-01",
            endDate = "2026-12-31"
        });

        // ─── Assert HTTP ───
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // ─── Assert DB ───
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();
        var project = await db.Projects.FindAsync(projectId);

        project!.Name.Should().Be(newName);
        project.Status.Should().Be(ProjectStatus.Active);
    }

    [Fact]
    public async Task AddMember_ValidIntern_AddsToProject()
    {
        // ─── Arrange ───
        await AuthenticateAsync();

        var adminId = DevSeeder.AdminId;
        var internshipId = Guid.NewGuid();
        var internId = Guid.NewGuid();
        var projectId = await SeedProjectAsync(internshipId, adminId, "Member Test");

        await SeedInternAsync(internId, internshipId);

        // ─── Act ───
        var response = await Client.PostAsJsonAsync(
            $"/api/projects/{projectId}/members",
            new { userId = internId, role = "Member" });

        // ─── Assert ───
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();
        var member = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == internId);

        member.Should().NotBeNull();
        member!.RemovedAt.Should().BeNull();
    }

    // ═══════════════════════════════════════════════════
    // Seeding helpers
    // ═══════════════════════════════════════════════════

    private async Task SeedInternshipAsync(Guid internshipId, Guid adminId)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();

        if (await db.Internships.AnyAsync(i => i.Id == internshipId))
            return;

        // ★ Đảm bảo admin tồn tại — dùng DevSeeder.AdminId
        adminId = DevSeeder.AdminId;

        db.Internships.Add(new Internship
        {
            Id = internshipId,
            Name = $"Test Internship {Guid.NewGuid():N}",
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31),
            Status = InternshipStatus.Active,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    private async Task<Guid> SeedProjectAsync(Guid internshipId, Guid adminId, string name)
    {
        await SeedInternshipAsync(internshipId, adminId);

        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();

        var projectId = Guid.NewGuid();
        db.Projects.Add(new Project
        {
            Id = projectId,
            InternshipId = internshipId,
            Name = name,
            Description = "Seeded for integration test",
            Status = ProjectStatus.Planned,
            CreatedBy = DevSeeder.AdminId,   // ★ dùng admin có thật
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Admin tạo → Admin là Lead
        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = DevSeeder.AdminId,       // ★ dùng admin có thật
            Role = MemberRole.Lead,
            JoinedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return projectId;
    }

    private async Task SeedInternAsync(Guid internId, Guid internshipId)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();

        if (await db.Users.AnyAsync(u => u.Id == internId))
            return;

        db.Users.Add(new User
        {
            Id = internId,
            Email = $"intern-{internId:N}@test.com",
            FullName = "Test Intern",
            PasswordHash = "hash",
            Role = UserRole.Intern,
            IsActive = true,
            RowVersion = Array.Empty<byte>(),
            InternProfile = new InternProfile
            {
                UserId = internId,
                UserRole = UserRole.Intern,
                CreatedAt = DateTime.UtcNow
            }
        });

        db.BatchInterns.Add(new BatchIntern
        {
            InternshipId = internshipId,
            InternId = internId,
            InternRole = UserRole.Intern,
            Status = BatchInternStatus.InProgress,
            AddedBy = DevSeeder.AdminId,      // ★ dùng admin có thật
            JoinedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════
    // Response wrappers
    // ═══════════════════════════════════════════════════

    private sealed record ApiEnvelope<T>(bool Success, T Data);
    private sealed record PagedEnvelope<T>(int TotalCount, List<T> Items);
    private sealed record ProjectResponseDto(Guid Id, string Name);
    private sealed record ProjectListItemDto(Guid Id, string Name);
}

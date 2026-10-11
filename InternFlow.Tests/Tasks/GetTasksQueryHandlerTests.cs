using FluentAssertions;
using InternFlow.Application.Features.Tasks;
using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Application.Features.Tasks.Queries.GetTasks;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Tests.Common.TestHelpers;
using Xunit;

using TaskStatus = InternFlow.Domain.Projects.TaskStatus;

namespace InternFlow.Tests.Tasks;

/// <summary>
/// Test cho GetTasksQueryHandler.
/// Bao gồm: phân quyền scope, filter, sort, paging.
/// </summary>
public class GetTasksQueryHandlerTests
{
    [Fact]
    public async Task GetTasks_Admin_SeesAllTasks()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);

        await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, TaskStatus.Todo, title: "T1");
        await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, TaskStatus.InProgress, title: "T2");
        await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, TaskStatus.Done, title: "T3");

        var handler = new GetTasksQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin));

        // Act
        var result = await handler.Handle(
            new GetTasksQuery(new TaskQueryParameters()), CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetTasks_Intern_SeesOnlyAssignedTasks()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var otherInternId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(otherInternId, UserRole.Intern));
        await db.SaveChangesAsync();

        await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, title: "My task");
        await TaskTestSetup.SeedTaskAsync(
            db, projectId, otherInternId, mentorId, title: "Other task");

        var handler = new GetTasksQueryHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern));

        // Act
        var result = await handler.Handle(
            new GetTasksQuery(new TaskQueryParameters()), CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].Title.Should().Be("My task");
    }

    [Fact]
    public async Task GetTasks_FilterByStatus_ReturnsCorrect()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);

        await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, TaskStatus.Todo);
        await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, TaskStatus.InProgress);
        await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, TaskStatus.InProgress);

        var handler = new GetTasksQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin));

        // Act
        var result = await handler.Handle(
            new GetTasksQuery(new TaskQueryParameters
            {
                Status = TaskStatus.InProgress
            }), CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetTasks_SortByDueDateAsc_ReturnsCorrect()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await TaskTestSetup.SeedTaskAsync(db, projectId, internId, mentorId,
            title: "Later", dueDate: today.AddDays(10));
        await TaskTestSetup.SeedTaskAsync(db, projectId, internId, mentorId,
            title: "Sooner", dueDate: today.AddDays(1));

        var handler = new GetTasksQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin));

        // Act
        var result = await handler.Handle(
            new GetTasksQuery(new TaskQueryParameters
            {
                SortBy = TaskSortFields.DueDate,
                SortDir = "asc"
            }), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Title)
            .Should().BeEquivalentTo(new[] { "Later", "Sooner" });
    }

    [Fact]
    public async Task GetTasks_PageSizeOver50_ClampsTo50()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();

        var handler = new GetTasksQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin));

        // Act
        var result = await handler.Handle(
            new GetTasksQuery(new TaskQueryParameters
            {
                PageSize = 1000
            }), CancellationToken.None);

        // Assert
        result.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task GetTasks_FilterByProjectId_ReturnsCorrect()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);

        // Project khác
        var otherInternshipId = Guid.NewGuid();
        var otherProjectId = Guid.NewGuid();
        db.Internships.Add(EntityFactory.CreateInternship(otherInternshipId, mentorId));
        var otherProject = EntityFactory.CreateProject(otherInternshipId, mentorId);
        otherProject.Id = otherProjectId;
        db.Projects.Add(otherProject);
        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = otherProjectId,
            UserId = internId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, title: "P1 Task");
        await TaskTestSetup.SeedTaskAsync(
            db, otherProjectId, internId, mentorId, title: "P2 Task");

        var handler = new GetTasksQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin));

        // Act
        var result = await handler.Handle(
            new GetTasksQuery(new TaskQueryParameters { ProjectId = projectId }),
            CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].Title.Should().Be("P1 Task");
    }

    [Fact]
    public async Task GetTasks_KeywordFilter_ReturnsMatching()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);

        await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, title: "Fix login bug");
        await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, title: "Fix logout bug");
        await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, title: "Add feature");

        var handler = new GetTasksQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin));

        // Act
        var result = await handler.Handle(
            new GetTasksQuery(new TaskQueryParameters { Keyword = "Fix" }),
            CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
    }
}

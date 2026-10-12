using FluentAssertions;
using InternFlow.Application.Features.Tasks.Commands.CreateTask;
using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Xunit;

using TaskStatus = InternFlow.Domain.Projects.TaskStatus;

namespace InternFlow.Tests.Tasks;

/// <summary>
/// Test cho CreateTaskCommandHandler.
/// Bao gồm: happy path, phân quyền, assignee rule.
/// </summary>
public class CreateTaskCommandHandlerTests
{
    [Fact]
    public async Task Create_LeadRole_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance,
            mediatorMock.Object,
            RealUnitOfWorkFactory.Create(db));

        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var request = new CreateTaskRequest(
            "Fix login bug", "Description",
            TaskPriority.High, dueDate, internId);

        // Act
        var result = await handler.Handle(
            new CreateTaskCommand(projectId, request), CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.ProjectId.Should().Be(projectId);
        result.Title.Should().Be("Fix login bug");
        result.Status.Should().Be(nameof(TaskStatus.Todo));
        result.Priority.Should().Be(nameof(TaskPriority.High));
        result.AssigneeId.Should().Be(internId);
        result.DueDate.Should().Be(dueDate);
        result.CreatedBy.Should().Be(mentorId);
        result.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Create_AdminRole_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var adminId = Guid.NewGuid();
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance,
            mediatorMock.Object,
             RealUnitOfWorkFactory.Create(db));

        var request = new CreateTaskRequest(
            "Task", null, TaskPriority.Medium, null, internId);

        // Act
        var result = await handler.Handle(
            new CreateTaskCommand(projectId, request), CancellationToken.None);

        // Assert
        result.CreatedBy.Should().Be(adminId);
    }

    [Fact]
    public async Task Create_ProjectNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var adminId = Guid.NewGuid();
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance,
            mediatorMock.Object,
             RealUnitOfWorkFactory.Create(db));

        var request = new CreateTaskRequest(
            "Task", null, TaskPriority.Medium, null, Guid.NewGuid());

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CreateTaskCommand(Guid.NewGuid(), request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_InternRole_ThrowsForbidden()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance,
            mediatorMock.Object,
             RealUnitOfWorkFactory.Create(db));

        var request = new CreateTaskRequest(
            "Task", null, TaskPriority.Medium, null, internId);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new CreateTaskCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_NonLeadMentor_ThrowsForbidden()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var otherMentorId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(otherMentorId, UserRole.Mentor));
        await db.SaveChangesAsync();
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(otherMentorId, UserRole.Mentor),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance,
            mediatorMock.Object,
             RealUnitOfWorkFactory.Create(db));

        var request = new CreateTaskRequest(
            "Task", null, TaskPriority.Medium, null, internId);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new CreateTaskCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_AssigneeNotIntern_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, _, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance,
            mediatorMock.Object,
             RealUnitOfWorkFactory.Create(db));

        // Assignee là Mentor — không phải Intern
        var request = new CreateTaskRequest(
            "Task", null, TaskPriority.Medium, null, mentorId);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateTaskCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_AssigneeNotMember_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, _, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var outsiderId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(outsiderId, UserRole.Intern));
        await db.SaveChangesAsync();
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance,
            mediatorMock.Object,
             RealUnitOfWorkFactory.Create(db));

        var request = new CreateTaskRequest(
            "Task", null, TaskPriority.Medium, null, outsiderId);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateTaskCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_InternNotInBatch_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            MockInternshipQueryFactory.Create(inBatch: false).Object,
            TestMapperFactory.Instance,
            mediatorMock.Object,
             RealUnitOfWorkFactory.Create(db));

        var request = new CreateTaskRequest(
            "Task", null, TaskPriority.Medium, null, internId);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateTaskCommand(projectId, request),
                CancellationToken.None));
    }
}

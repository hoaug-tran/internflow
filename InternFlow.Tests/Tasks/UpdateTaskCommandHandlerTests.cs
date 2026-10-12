using FluentAssertions;
using InternFlow.Application.Features.Tasks.Commands.UpdateTask;
using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Xunit;

namespace InternFlow.Tests.Tasks;

/// <summary>
/// Test cho UpdateTaskCommandHandler.
/// Bao gồm: update fields, đổi assignee, phân quyền.
/// </summary>
public class UpdateTaskCommandHandlerTests
{
    [Fact]
    public async Task Update_Mentor_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var taskId = await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new UpdateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object,
RealUnitOfWorkFactory.Create(db));

        var request = new UpdateTaskRequest(
            "Updated Title", "Updated Desc",
            TaskPriority.High, null, internId);

        // Act
        var result = await handler.Handle(
            new UpdateTaskCommand(taskId, request), CancellationToken.None);

        // Assert
        result.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Desc");
        result.Priority.Should().Be(nameof(TaskPriority.High));
    }

    [Fact]
    public async Task Update_ChangeAssignee_ValidIntern_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var newInternId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(newInternId, UserRole.Intern));
        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = newInternId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var taskId = await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new UpdateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object,
RealUnitOfWorkFactory.Create(db));

        var request = new UpdateTaskRequest(
            "Task", null, TaskPriority.Medium, null, newInternId);

        // Act
        var result = await handler.Handle(
            new UpdateTaskCommand(taskId, request), CancellationToken.None);

        // Assert
        result.AssigneeId.Should().Be(newInternId);
    }

    [Fact]
    public async Task Update_ChangeAssignee_NotIntern_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var taskId = await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new UpdateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object,
RealUnitOfWorkFactory.Create(db));

        // Assignee mới là Mentor — không phải Intern
        var request = new UpdateTaskRequest(
            "Task", null, TaskPriority.Medium, null, mentorId);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new UpdateTaskCommand(taskId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Update_Intern_ThrowsForbidden()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var taskId = await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new UpdateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
            mediatorMock.Object,
RealUnitOfWorkFactory.Create(db));

        var request = new UpdateTaskRequest(
            "Updated", null, TaskPriority.Medium, null, internId);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new UpdateTaskCommand(taskId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Update_TaskNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var adminId = Guid.NewGuid();
        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new UpdateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            mediatorMock.Object,
RealUnitOfWorkFactory.Create(db));

        var request = new UpdateTaskRequest(
            "Task", null, TaskPriority.Medium, null, Guid.NewGuid());

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new UpdateTaskCommand(Guid.NewGuid(), request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Update_NonMemberMentor_ThrowsForbidden()
    {
        // Arrange — Mentor không thuộc project
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var otherMentorId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(otherMentorId, UserRole.Mentor));
        await db.SaveChangesAsync();

        var taskId = await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new UpdateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(otherMentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object,
RealUnitOfWorkFactory.Create(db));

        var request = new UpdateTaskRequest(
            "Task", null, TaskPriority.Medium, null, internId);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new UpdateTaskCommand(taskId, request),
                CancellationToken.None));
    }
}

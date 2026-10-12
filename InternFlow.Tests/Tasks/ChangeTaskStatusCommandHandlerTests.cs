using FluentAssertions;
using InternFlow.Application.Features.Tasks.Commands.ChangeTaskStatus;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Xunit;

using TaskStatus = InternFlow.Domain.Projects.TaskStatus;

namespace InternFlow.Tests.Tasks;

/// <summary>
/// Test cho ChangeTaskStatusCommandHandler.
/// Bao gồm: luồng hợp lệ, chặn intern tự duyệt, skip step, terminal state.
/// </summary>
public class ChangeTaskStatusCommandHandlerTests
{
    [Fact]
    public async Task ChangeStatus_Intern_TodoToInProgress_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var taskId = await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, internId, TaskStatus.Todo);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
            mediatorMock.Object,
RealUnitOfWorkFactory.Create(db));

        // Act
        var result = await handler.Handle(
            new ChangeTaskStatusCommand(taskId, TaskStatus.InProgress),
            CancellationToken.None);

        // Assert
        result.Status.Should().Be(nameof(TaskStatus.InProgress));
    }

    [Fact]
    public async Task ChangeStatus_Intern_InProgressToInReview_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var taskId = await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, internId, TaskStatus.InProgress);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
            mediatorMock.Object,
RealUnitOfWorkFactory.Create(db));

        // Act
        var result = await handler.Handle(
            new ChangeTaskStatusCommand(taskId, TaskStatus.InReview),
            CancellationToken.None);

        // Assert
        result.Status.Should().Be(nameof(TaskStatus.InReview));
    }

    [Fact]
    public async Task ChangeStatus_Intern_InReviewToDone_ThrowsForbidden()
    {
        // Arrange — Intern KHÔNG được tự duyệt
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var taskId = await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, internId, TaskStatus.InReview);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
            mediatorMock.Object,
RealUnitOfWorkFactory.Create(db));

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(
                new ChangeTaskStatusCommand(taskId, TaskStatus.Done),
                CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatus_Mentor_InReviewToDone_Success_SetCompletedAt()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var taskId = await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, TaskStatus.InReview);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object,
             RealUnitOfWorkFactory.Create(db));

        // Act
        var result = await handler.Handle(
            new ChangeTaskStatusCommand(taskId, TaskStatus.Done),
            CancellationToken.None);

        // Assert
        result.Status.Should().Be(nameof(TaskStatus.Done));
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ChangeStatus_Mentor_InReviewToInProgress_Success()
    {
        // Arrange — Mentor trả task lại
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var taskId = await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, TaskStatus.InReview);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object,
RealUnitOfWorkFactory.Create(db));

        // Act
        var result = await handler.Handle(
            new ChangeTaskStatusCommand(taskId, TaskStatus.InProgress),
            CancellationToken.None);

        // Assert
        result.Status.Should().Be(nameof(TaskStatus.InProgress));
    }

    [Fact]
    public async Task ChangeStatus_SkipStep_ThrowsConflict()
    {
        // Arrange — Todo → Done (bỏ qua bước)
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var taskId = await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, TaskStatus.Todo);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object,
RealUnitOfWorkFactory.Create(db));

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new ChangeTaskStatusCommand(taskId, TaskStatus.Done),
                CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatus_TerminalState_ThrowsConflict()
    {
        // Arrange — Done → Todo (terminal)
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var taskId = await TaskTestSetup.SeedTaskAsync(
            db, projectId, internId, mentorId, TaskStatus.Done);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object,
RealUnitOfWorkFactory.Create(db));

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new ChangeTaskStatusCommand(taskId, TaskStatus.Todo),
                CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatus_Intern_OtherTask_ThrowsNotFound()
    {
        // Arrange — Intern thao tác task không phải của mình
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await TaskTestSetup
            .SetupProjectWithInternAsync(db);
        var otherInternId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(otherInternId, UserRole.Intern));
        await db.SaveChangesAsync();

        var taskId = await TaskTestSetup.SeedTaskAsync(
            db, projectId, otherInternId, mentorId, TaskStatus.Todo);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
            mediatorMock.Object,
RealUnitOfWorkFactory.Create(db));

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new ChangeTaskStatusCommand(taskId, TaskStatus.InProgress),
                CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatus_TaskNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var adminId = Guid.NewGuid();
        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            mediatorMock.Object,
RealUnitOfWorkFactory.Create(db));

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new ChangeTaskStatusCommand(Guid.NewGuid(), TaskStatus.InProgress),
                CancellationToken.None));
    }
}

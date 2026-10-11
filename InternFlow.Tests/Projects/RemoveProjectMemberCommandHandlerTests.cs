using FluentAssertions;
using InternFlow.Application.Features.Projects.Commands.RemoveProjectMember;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

using TaskStatus = InternFlow.Domain.Projects.TaskStatus;

namespace InternFlow.Tests.Projects;

/// <summary>
/// Test cho RemoveProjectMemberCommandHandler.
/// Bao gồm: soft delete, chặn xóa Lead cuối, chặn xóa member còn task.
/// </summary>
public class RemoveProjectMemberCommandHandlerTests
{
    [Fact]
    public async Task RemoveMember_AdminRemovesMember_SoftDeletes()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var memberId = Guid.NewGuid();

        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = memberId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new RemoveProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            mediatorMock.Object);

        // Act
        await handler.Handle(
            new RemoveProjectMemberCommand(projectId, memberId),
            CancellationToken.None);

        // Assert — soft delete
        var member = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == memberId);
        member.Should().NotBeNull();
        member!.RemovedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveMember_ProjectNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var adminId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(adminId, UserRole.Admin));
        await db.SaveChangesAsync();

        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new RemoveProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            mediatorMock.Object);

        // Act + Assert — EnsureCanManageProjectAsync sẽ ném NotFound
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new RemoveProjectMemberCommand(Guid.NewGuid(), Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task RemoveMember_MemberNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);

        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new RemoveProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            mediatorMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new RemoveProjectMemberCommand(projectId, Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task RemoveMember_AlreadyRemoved_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var memberId = Guid.NewGuid();

        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = memberId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow.AddDays(-10),
            RemovedAt = DateTime.UtcNow.AddDays(-5)
        });
        await db.SaveChangesAsync();

        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new RemoveProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            mediatorMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new RemoveProjectMemberCommand(projectId, memberId),
                CancellationToken.None));
    }

    [Fact]
    public async Task RemoveMember_LastLead_ThrowsConflict()
    {
        // Arrange — Admin là Lead duy nhất của project
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);

        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new RemoveProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            mediatorMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new RemoveProjectMemberCommand(projectId, adminId),
                CancellationToken.None));
    }

    [Fact]
    public async Task RemoveMember_HasOpenTasks_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var memberId = Guid.NewGuid();

        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = memberId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow
        });

        // Member có task chưa Done
        db.Tasks.Add(new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            AssigneeId = memberId,
            Title = "Task 1",
            Status = TaskStatus.Todo,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new RemoveProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            mediatorMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new RemoveProjectMemberCommand(projectId, memberId),
                CancellationToken.None));
    }

    [Fact]
    public async Task RemoveMember_OnlyCancelledTasks_Success()
    {
        // Arrange — Task Cancelled không tính là open
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var memberId = Guid.NewGuid();

        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = memberId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow
        });

        db.Tasks.Add(new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            AssigneeId = memberId,
            Title = "Cancelled Task",
            Status = TaskStatus.Cancelled,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new RemoveProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            mediatorMock.Object);

        // Act
        await handler.Handle(
            new RemoveProjectMemberCommand(projectId, memberId),
            CancellationToken.None);

        // Assert
        var member = await db.ProjectMembers
            .FirstAsync(m => m.ProjectId == projectId && m.UserId == memberId);
        member.RemovedAt.Should().NotBeNull();
    }
}

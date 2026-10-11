using FluentAssertions;
using InternFlow.Application.Features.WorkLogs.Commands.DeleteWorkLog;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InternFlow.Tests.WorkLogs;

/// <summary>
/// Test cho DeleteWorkLogCommandHandler.
/// Bao gồm: soft delete, ownership, edit window.
/// </summary>
public class DeleteWorkLogCommandHandlerTests
{
    [Fact]
    public async Task Delete_ValidRequest_SoftDeletes()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId);

        var handler = new DeleteWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern));

        // Act
        await handler.Handle(
            new DeleteWorkLogCommand(workLogId), CancellationToken.None);

        // Assert — soft delete
        var saved = await db.WorkLogs.FindAsync(workLogId);
        saved.Should().NotBeNull();
        saved!.DeletedAt.Should().NotBeNull();
        saved.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_WorkLogNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internId = Guid.NewGuid();

        var handler = new DeleteWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern));

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new DeleteWorkLogCommand(Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Delete_NotOwner_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId);
        var otherInternId = Guid.NewGuid();

        var handler = new DeleteWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(otherInternId, UserRole.Intern));

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new DeleteWorkLogCommand(workLogId),
                CancellationToken.None));
    }

    [Fact]
    public async Task Delete_PastEditWindow_ThrowsConflict()
    {
        // Arrange — Worklog tạo 10 ngày trước
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId,
            createdAt: DateTime.UtcNow.AddDays(-10));

        var handler = new DeleteWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern));

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new DeleteWorkLogCommand(workLogId),
                CancellationToken.None));
    }

    [Fact]
    public async Task Delete_AlreadyDeleted_ThrowsNotFound()
    {
        // Arrange — Worklog đã bị xóa mềm
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId);

        var workLog = await db.WorkLogs.FindAsync(workLogId);
        workLog!.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var handler = new DeleteWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern));

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new DeleteWorkLogCommand(workLogId),
                CancellationToken.None));
    }
}

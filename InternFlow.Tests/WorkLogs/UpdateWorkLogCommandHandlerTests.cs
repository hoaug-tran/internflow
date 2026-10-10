using FluentAssertions;
using InternFlow.Application.Features.WorkLogs.Commands.UpdateWorkLog;
using InternFlow.Application.Features.WorkLogs.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InternFlow.Tests.WorkLogs;

/// <summary>
/// Test cho UpdateWorkLogCommandHandler.
/// Bao gồm: happy path, ownership, edit window, giới hạn giờ.
/// </summary>
public class UpdateWorkLogCommandHandlerTests
{
    [Fact]
    public async Task Update_ValidRequest_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId, hours: 4m, description: "Old");

        var handler = new UpdateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance);

        var request = new UpdateWorkLogRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            6m,
            "Updated description");

        // Act
        var result = await handler.Handle(
            new UpdateWorkLogCommand(workLogId, request), CancellationToken.None);

        // Assert
        result.Hours.Should().Be(6m);
        result.Description.Should().Be("Updated description");

        var saved = await db.WorkLogs.FindAsync(workLogId);
        saved!.Hours.Should().Be(6m);
        saved.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_WorkLogNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internId = Guid.NewGuid();

        var handler = new UpdateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance);

        var request = new UpdateWorkLogRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            4m,
            "Test");

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new UpdateWorkLogCommand(Guid.NewGuid(), request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Update_NotOwner_ThrowsNotFound()
    {
        // Arrange — Intern khác cố sửa worklog
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId);
        var otherInternId = Guid.NewGuid();

        var handler = new UpdateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(otherInternId, UserRole.Intern),
            TestMapperFactory.Instance);

        var request = new UpdateWorkLogRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            6m,
            "Hacked");

        // Act + Assert — trả NotFound để không tiết lộ sự tồn tại
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new UpdateWorkLogCommand(workLogId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Update_PastEditWindow_ThrowsConflict()
    {
        // Arrange — Worklog tạo 10 ngày trước
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId,
            createdAt: DateTime.UtcNow.AddDays(-10));

        var handler = new UpdateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance);

        var request = new UpdateWorkLogRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            6m,
            "Late edit");

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new UpdateWorkLogCommand(workLogId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Update_TotalHoursExceed12_ThrowsConflict()
    {
        // Arrange — Có 9h ở task khác cùng ngày, sửa worklog này thành 4h → 13h
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        var otherTaskId = Guid.NewGuid();
        var task = await db.Tasks.FindAsync(taskId);
        db.Tasks.Add(new TaskItem
        {
            Id = otherTaskId,
            ProjectId = projectId,
            AssigneeId = internId,
            Title = "Other",
            Status = InternFlow.Domain.Projects.TaskStatus.InProgress,
            Priority = InternFlow.Domain.Projects.TaskPriority.Medium,
            CreatedBy = task!.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await WorkLogTestSetup.SeedWorkLogAsync(
            db, otherTaskId, internId, workDate: today, hours: 9m);

        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId, workDate: today, hours: 2m);

        var handler = new UpdateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance);

        var request = new UpdateWorkLogRequest(today, 4m, "Updated");

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new UpdateWorkLogCommand(workLogId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Update_SameHours_NoConflict()
    {
        // Arrange — Cập nhật description, giữ nguyên hours
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId, hours: 4m, description: "Old");

        var handler = new UpdateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance);

        var request = new UpdateWorkLogRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            4m,
            "New description");

        // Act
        var result = await handler.Handle(
            new UpdateWorkLogCommand(workLogId, request), CancellationToken.None);

        // Assert
        result.Description.Should().Be("New description");
        result.Hours.Should().Be(4m);
    }
}

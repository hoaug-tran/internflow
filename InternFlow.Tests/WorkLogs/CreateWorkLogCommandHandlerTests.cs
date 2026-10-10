using FluentAssertions;
using InternFlow.Application.Features.WorkLogs.Commands.CreateWorkLog;
using InternFlow.Application.Features.WorkLogs.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

using TaskStatus = InternFlow.Domain.Projects.TaskStatus;

namespace InternFlow.Tests.WorkLogs;

/// <summary>
/// Test cho CreateWorkLogCommandHandler.
/// Bao gồm: happy path, ownership, task status, batch, giới hạn giờ.
/// </summary>
public class CreateWorkLogCommandHandlerTests
{
    [Fact]
    public async Task Create_ValidRequest_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        var handler = new CreateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        var request = new CreateWorkLogRequest(
            taskId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            4m,
            "Đã fix xong bug login");

        // Act
        var result = await handler.Handle(
            new CreateWorkLogCommand(request), CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.TaskItemId.Should().Be(taskId);
        result.UserId.Should().Be(internId);
        result.Hours.Should().Be(4m);
        result.Description.Should().Be("Đã fix xong bug login");
        result.TaskTitle.Should().Be("Task for worklog");

        var saved = await db.WorkLogs.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Create_TaskNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internId = Guid.NewGuid();

        var handler = new CreateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        var request = new CreateWorkLogRequest(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            4m,
            "Test");

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CreateWorkLogCommand(request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_NotOwner_ThrowsForbidden()
    {
        // Arrange — Intern A ghi worklog cho task của Intern B
        await using var db = TestDbContextFactory.Create();
        var (_, _, _, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var otherInternId = Guid.NewGuid();

        var handler = new CreateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(otherInternId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        var request = new CreateWorkLogRequest(
            taskId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            4m,
            "Test");

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new CreateWorkLogCommand(request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_CancelledTask_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db, TaskStatus.Cancelled);

        var handler = new CreateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        var request = new CreateWorkLogRequest(
            taskId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            4m,
            "Test");

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateWorkLogCommand(request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_TodoTask_ThrowsConflict()
    {
        // Arrange — Task chưa bắt đầu
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db, TaskStatus.Todo);

        var handler = new CreateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        var request = new CreateWorkLogRequest(
            taskId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            4m,
            "Test");

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateWorkLogCommand(request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_FutureWorkDate_ThrowsBadRequest()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        var handler = new CreateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        var request = new CreateWorkLogRequest(
            taskId,
            DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),   // tương lai
            4m,
            "Test");

        // Act + Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            handler.Handle(new CreateWorkLogCommand(request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_InternNotActive_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        var handler = new CreateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.CreateForWorkLog(
                internActive: false).Object,
            TestMapperFactory.Instance);

        var request = new CreateWorkLogRequest(
            taskId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            4m,
            "Test");

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateWorkLogCommand(request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_WorkDateOutsideBatch_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        var handler = new CreateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.CreateForWorkLog(
                workDateInBatch: false).Object,
            TestMapperFactory.Instance);

        var request = new CreateWorkLogRequest(
            taskId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            4m,
            "Test");

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateWorkLogCommand(request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_TaskDoneWithinGracePeriod_Success()
    {
        // Arrange — Task Done 1 ngày trước, trong grace period 3 ngày
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db, TaskStatus.Done);

        // Chỉnh CompletedAt về 1 ngày trước
        var task = await db.Tasks.FindAsync(taskId);
        task!.CompletedAt = DateTime.UtcNow.AddDays(-1);
        await db.SaveChangesAsync();

        var handler = new CreateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        var request = new CreateWorkLogRequest(
            taskId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            4m,
            "Ghi bù");

        // Act
        var result = await handler.Handle(
            new CreateWorkLogCommand(request), CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Create_TaskDonePastGracePeriod_ThrowsConflict()
    {
        // Arrange — Task Done 5 ngày trước, quá grace period
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db, TaskStatus.Done);

        var task = await db.Tasks.FindAsync(taskId);
        task!.CompletedAt = DateTime.UtcNow.AddDays(-5);
        await db.SaveChangesAsync();

        var handler = new CreateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        var request = new CreateWorkLogRequest(
            taskId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            4m,
            "Test");

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateWorkLogCommand(request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_TotalHoursExceed12_ThrowsConflict()
    {
        // Arrange — Đã có 9h, thêm 4h → 13h > 12
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        // Tạo task khác để seed worklog tránh trùng unique index
        var otherTaskId = Guid.NewGuid();
        var task = await db.Tasks.FindAsync(taskId);
        db.Tasks.Add(new TaskItem
        {
            Id = otherTaskId,
            ProjectId = task!.ProjectId,
            AssigneeId = internId,
            Title = "Other task",
            Status = TaskStatus.InProgress,
            Priority = TaskPriority.Medium,
            CreatedBy = task.CreatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        await WorkLogTestSetup.SeedWorkLogAsync(
            db, otherTaskId, internId,
            workDate: DateOnly.FromDateTime(DateTime.UtcNow),
            hours: 9m);

        var handler = new CreateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        var request = new CreateWorkLogRequest(
            taskId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            4m,
            "Test");

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateWorkLogCommand(request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_DuplicateSameTaskSameDate_ThrowsConflict()
    {
        // Arrange — Đã có worklog cho task này hôm nay
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId, workDate: today, hours: 2m);

        var handler = new CreateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        var request = new CreateWorkLogRequest(
            taskId, today, 4m, "Test");

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateWorkLogCommand(request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_ProjectOnHold_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        var project = await db.Projects.FindAsync(projectId);
        project!.Status = ProjectStatus.OnHold;
        await db.SaveChangesAsync();

        var handler = new CreateWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        var request = new CreateWorkLogRequest(
            taskId,
            DateOnly.FromDateTime(DateTime.UtcNow),
            4m,
            "Test");

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateWorkLogCommand(request),
                CancellationToken.None));
    }
}

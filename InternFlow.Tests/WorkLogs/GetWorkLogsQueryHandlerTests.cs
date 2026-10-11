using FluentAssertions;
using InternFlow.Application.Features.WorkLogs.DTOs;
using InternFlow.Application.Features.WorkLogs.Queries.GetWorkLogs;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Models;
using InternFlow.Tests.Common.TestHelpers;
using Xunit;

namespace InternFlow.Tests.WorkLogs;

/// <summary>
/// Test cho GetWorkLogsQueryHandler.
/// Bao gồm: phân quyền scope, filter.
/// </summary>
public class GetWorkLogsQueryHandlerTests
{
    [Fact]
    public async Task GetWorkLogs_Intern_SeesOnlyOwn()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var otherInternId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(otherInternId, UserRole.Intern));
        await db.SaveChangesAsync();

        await WorkLogTestSetup.SeedWorkLogAsync(db, taskId, internId);
        await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, otherInternId,
            workDate: DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1));

        var handler = new GetWorkLogsQueryHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object);

        // Act
        var result = await handler.Handle(
            new GetWorkLogsQuery(new WorkLogQueryParameters()),
            CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].UserId.Should().Be(internId);
    }

    [Fact]
    public async Task GetWorkLogs_Admin_SeesAll()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        await WorkLogTestSetup.SeedWorkLogAsync(db, taskId, internId);

        var handler = new GetWorkLogsQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin),
            MockInternshipQueryFactory.Create().Object);

        // Act
        var result = await handler.Handle(
            new GetWorkLogsQuery(new WorkLogQueryParameters()),
            CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetWorkLogs_Mentor_SeesOnlyAssignedInterns()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var otherInternId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(otherInternId, UserRole.Intern));
        await db.SaveChangesAsync();

        await WorkLogTestSetup.SeedWorkLogAsync(db, taskId, internId);
        await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, otherInternId,
            workDate: DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1));

        // Mentor chỉ phụ trách internId
        var handler = new GetWorkLogsQueryHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            MockInternshipQueryFactory.CreateForMentorView(internId).Object);

        // Act
        var result = await handler.Handle(
            new GetWorkLogsQuery(new WorkLogQueryParameters()),
            CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].UserId.Should().Be(internId);
    }

    [Fact]
    public async Task GetWorkLogs_FilterByDateRange_ReturnsCorrect()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId, workDate: today.AddDays(-10));
        await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId, workDate: today.AddDays(-5));
        await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId, workDate: today);

        var handler = new GetWorkLogsQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin),
            MockInternshipQueryFactory.Create().Object);

        // Act — lấy từ 7 ngày trước đến nay
        var result = await handler.Handle(
            new GetWorkLogsQuery(new WorkLogQueryParameters
            {
                From = today.AddDays(-7),
                To = today
            }),
            CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetWorkLogs_FilterByProjectId_ReturnsCorrect()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        await WorkLogTestSetup.SeedWorkLogAsync(db, taskId, internId);

        var handler = new GetWorkLogsQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin),
            MockInternshipQueryFactory.Create().Object);

        // Act
        var result = await handler.Handle(
            new GetWorkLogsQuery(new WorkLogQueryParameters
            {
                ProjectId = projectId
            }),
            CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
    }
}

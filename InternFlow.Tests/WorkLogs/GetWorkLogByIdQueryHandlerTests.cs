using FluentAssertions;
using InternFlow.Application.Features.WorkLogs.Queries.GetWorkLogById;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Xunit;

namespace InternFlow.Tests.WorkLogs;

/// <summary>
/// Test cho GetWorkLogByIdQueryHandler.
/// Bao gồm: phân quyền scope theo role.
/// </summary>
public class GetWorkLogByIdQueryHandlerTests
{
    [Fact]
    public async Task GetById_Intern_OwnWorkLog_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId);

        var handler = new GetWorkLogByIdQueryHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        // Act
        var result = await handler.Handle(
            new GetWorkLogByIdQuery(workLogId), CancellationToken.None);

        // Assert
        result.Id.Should().Be(workLogId);
        result.UserId.Should().Be(internId);
    }

    [Fact]
    public async Task GetById_Intern_OtherWorkLog_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId);
        var otherInternId = Guid.NewGuid();

        var handler = new GetWorkLogByIdQueryHandler(
            db,
            MockCurrentUserFactory.Create(otherInternId, UserRole.Intern),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new GetWorkLogByIdQuery(workLogId),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetById_Admin_AnyWorkLog_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId);

        var handler = new GetWorkLogByIdQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        // Act
        var result = await handler.Handle(
            new GetWorkLogByIdQuery(workLogId), CancellationToken.None);

        // Assert
        result.Id.Should().Be(workLogId);
    }

    [Fact]
    public async Task GetById_Mentor_AssignedIntern_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId);

        var handler = new GetWorkLogByIdQueryHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            MockInternshipQueryFactory.CreateForMentorView(internId).Object,
            TestMapperFactory.Instance);

        // Act
        var result = await handler.Handle(
            new GetWorkLogByIdQuery(workLogId), CancellationToken.None);

        // Assert
        result.Id.Should().Be(workLogId);
    }

    [Fact]
    public async Task GetById_Mentor_NotAssignedIntern_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);
        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId);

        // Mentor không phụ trách internId
        var handler = new GetWorkLogByIdQueryHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            MockInternshipQueryFactory.CreateForMentorView().Object,
            TestMapperFactory.Instance);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new GetWorkLogByIdQuery(workLogId),
                CancellationToken.None));
    }

    [Fact]
    public async Task GetById_WorkLogNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var adminId = Guid.NewGuid();

        var handler = new GetWorkLogByIdQueryHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            MockInternshipQueryFactory.Create().Object,
            TestMapperFactory.Instance);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new GetWorkLogByIdQuery(Guid.NewGuid()),
                CancellationToken.None));
    }
}

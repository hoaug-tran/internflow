using FluentAssertions;
using InternFlow.Application.Features.WorkLogs.Commands.RestoreWorkLog;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InternFlow.Tests.WorkLogs;

public class RestoreWorkLogCommandHandlerTests
{
    [Fact]
    public async Task Restore_ValidDeletedWorkLog_Success()
    {
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId);

        var workLog = await db.WorkLogs.FindAsync(workLogId);
        workLog!.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var handler = new RestoreWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
            RealUnitOfWorkFactory.Create(db));

        var result = await handler.Handle(
            new RestoreWorkLogCommand(workLogId), CancellationToken.None);

        result.Id.Should().Be(workLogId);

        var restored = await db.WorkLogs.FindAsync(workLogId);
        restored!.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Restore_NotDeleted_ThrowsNotFound()
    {
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId);

        var handler = new RestoreWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
            RealUnitOfWorkFactory.Create(db));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new RestoreWorkLogCommand(workLogId),
                CancellationToken.None));
    }

    [Fact]
    public async Task Restore_NotFound_ThrowsNotFound()
    {
        await using var db = TestDbContextFactory.Create();
        var internId = Guid.NewGuid();

        var handler = new RestoreWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
            RealUnitOfWorkFactory.Create(db));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new RestoreWorkLogCommand(Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Restore_NotOwner_ThrowsNotFound()
    {
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId);

        var workLog = await db.WorkLogs.FindAsync(workLogId);
        workLog!.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var otherInternId = Guid.NewGuid();

        var handler = new RestoreWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(otherInternId, UserRole.Intern),
            TestMapperFactory.Instance,
            RealUnitOfWorkFactory.Create(db));

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new RestoreWorkLogCommand(workLogId),
                CancellationToken.None));
    }

    [Fact]
    public async Task Restore_PastRestoreWindow_ThrowsConflict()
    {
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, _, taskId) = await WorkLogTestSetup
            .SetupTaskForWorkLogAsync(db);

        var workLogId = await WorkLogTestSetup.SeedWorkLogAsync(
            db, taskId, internId);

        var workLog = await db.WorkLogs.FindAsync(workLogId);
        workLog!.DeletedAt = DateTime.UtcNow.AddDays(-10);
        await db.SaveChangesAsync();

        var handler = new RestoreWorkLogCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
            RealUnitOfWorkFactory.Create(db));

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new RestoreWorkLogCommand(workLogId),
                CancellationToken.None));
    }
}

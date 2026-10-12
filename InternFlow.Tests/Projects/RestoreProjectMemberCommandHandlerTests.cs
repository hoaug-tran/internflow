using FluentAssertions;
using InternFlow.Application.Features.Projects.Commands.RestoreProjectMember;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InternFlow.Tests.Projects;

public class RestoreProjectMemberCommandHandlerTests
{
    [Fact]
    public async Task Restore_ValidRemovedMember_Success()
    {
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var memberId = Guid.NewGuid();
        db.Users.Add(EntityFactory.CreateUser(memberId, UserRole.Mentor));

        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = memberId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow.AddDays(-10),
            RemovedAt = DateTime.UtcNow.AddDays(-5)
        });
        await db.SaveChangesAsync();

        var handler = new RestoreProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            MockMediatorFactory.CreateNoOp().Object,
            db);

        var result = await handler.Handle(
            new RestoreProjectMemberCommand(projectId, memberId),
            CancellationToken.None);

        result.UserId.Should().Be(memberId);

        var restored = await db.ProjectMembers
            .FirstAsync(m => m.ProjectId == projectId && m.UserId == memberId);
        restored.RemovedAt.Should().BeNull();
    }

    [Fact]
    public async Task Restore_MemberNotFound_ThrowsNotFound()
    {
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);

        var handler = new RestoreProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            MockMediatorFactory.CreateNoOp().Object,
            db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new RestoreProjectMemberCommand(projectId, Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Restore_NotRemoved_ThrowsConflict()
    {
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

        var handler = new RestoreProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            MockMediatorFactory.CreateNoOp().Object,
            db);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new RestoreProjectMemberCommand(projectId, memberId),
                CancellationToken.None));
    }

    [Fact]
    public async Task Restore_ProjectNotFound_ThrowsNotFound()
    {
        await using var db = TestDbContextFactory.Create();
        var adminId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(adminId, UserRole.Admin));
        await db.SaveChangesAsync();

        var handler = new RestoreProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            MockMediatorFactory.CreateNoOp().Object,
            db);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new RestoreProjectMemberCommand(Guid.NewGuid(), Guid.NewGuid()),
                CancellationToken.None));
    }

    [Fact]
    public async Task Restore_NonAdminNonLead_ThrowsForbidden()
    {
        await using var db = TestDbContextFactory.Create();
        var (_, _, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var memberId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = memberId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow.AddDays(-10),
            RemovedAt = DateTime.UtcNow.AddDays(-5)
        });
        await db.SaveChangesAsync();

        var handler = new RestoreProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(otherUserId, UserRole.Mentor),
            TestMapperFactory.Instance,
            MockMediatorFactory.CreateNoOp().Object,
            db);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(
                new RestoreProjectMemberCommand(projectId, memberId),
                CancellationToken.None));
    }
}

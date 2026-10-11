using FluentAssertions;
using InternFlow.Application.Features.Projects.Queries.GetProjectMembers;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InternFlow.Tests.Projects;

/// <summary>
/// Test cho GetProjectMembersQueryHandler.
/// Bao gồm: chỉ trả member active, sắp xếp theo JoinedAt, phân quyền scope.
/// </summary>
public class GetProjectMembersQueryHandlerTests
{
    [Fact]
    public async Task GetMembers_ReturnsOnlyActive()
    {
        // Arrange — Helper seed Admin làm Lead, nên tổng cộng 2 active member
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var activeId = Guid.NewGuid();
        var removedId = Guid.NewGuid();

        db.Users.AddRange(
            EntityFactory.CreateUser(activeId, UserRole.Mentor),
            EntityFactory.CreateUser(removedId, UserRole.Mentor));

        db.ProjectMembers.AddRange(
            new ProjectMember
            {
                ProjectId = projectId,
                UserId = activeId,
                Role = MemberRole.Member,
                JoinedAt = DateTime.UtcNow
            },
            new ProjectMember
            {
                ProjectId = projectId,
                UserId = removedId,
                Role = MemberRole.Member,
                JoinedAt = DateTime.UtcNow,
                RemovedAt = DateTime.UtcNow.AddDays(-1)
            });
        await db.SaveChangesAsync();

        var handler = new GetProjectMembersQueryHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin));

        // Act
        var result = await handler.Handle(
            new GetProjectMembersQuery(projectId), CancellationToken.None);

        // Assert — Admin (Lead) + activeId (Member); removedId bị loại
        result.Should().HaveCount(2);
        result.Select(m => m.UserId).Should().Contain(adminId);
        result.Select(m => m.UserId).Should().Contain(activeId);
        result.Select(m => m.UserId).Should().NotContain(removedId);
    }

    [Fact]
    public async Task GetMembers_OrderedByJoinedAt()
    {
        // Arrange — override JoinedAt để thứ tự xác định
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);

        var member1Id = Guid.NewGuid();
        var member2Id = Guid.NewGuid();

        db.Users.AddRange(
            EntityFactory.CreateUser(member1Id, UserRole.Mentor),
            EntityFactory.CreateUser(member2Id, UserRole.Mentor));

        var baseTime = DateTime.UtcNow;

        // Admin JoinedAt = baseTime
        var adminMember = await db.ProjectMembers
            .FirstAsync(m => m.ProjectId == projectId && m.UserId == adminId);
        adminMember.JoinedAt = baseTime;

        db.ProjectMembers.AddRange(
            new ProjectMember
            {
                ProjectId = projectId,
                UserId = member1Id,
                Role = MemberRole.Member,
                JoinedAt = baseTime.AddDays(-2)   // Cũ nhất
            },
            new ProjectMember
            {
                ProjectId = projectId,
                UserId = member2Id,
                Role = MemberRole.Member,
                JoinedAt = baseTime.AddDays(1)    // Mới nhất
            });
        await db.SaveChangesAsync();

        var handler = new GetProjectMembersQueryHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin));

        // Act
        var result = await handler.Handle(
            new GetProjectMembersQuery(projectId), CancellationToken.None);

        // Assert — OrderBy JoinedAt asc
        result.Should().HaveCount(3);
        result[0].UserId.Should().Be(member1Id);   // -2 days
        result[1].UserId.Should().Be(adminId);     // base
        result[2].UserId.Should().Be(member2Id);   // +1 day
    }

    [Fact]
    public async Task GetMembers_ProjectNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var adminId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(adminId, UserRole.Admin));
        await db.SaveChangesAsync();

        var handler = new GetProjectMembersQueryHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin));

        // Act + Assert — EnsureCanViewProjectAsync ném NotFound
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new GetProjectMembersQuery(Guid.NewGuid()),
                CancellationToken.None));
    }
}

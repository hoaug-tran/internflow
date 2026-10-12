using FluentAssertions;
using InternFlow.Application.Features.Projects.Commands.AddProjectMember;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InternFlow.Tests.Projects;

/// <summary>
/// Test cho AddProjectMemberCommandHandler.
/// Bao gồm: happy path, ràng buộc role/batch, duplicate, reactivate, phân quyền.
/// </summary>
public class AddProjectMemberCommandHandlerTests
{
    [Fact]
    public async Task AddMember_AdminAddsMentor_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var mentorId = await ProjectTestSetup.SeedMentorAsync(db);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new AddProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            mediatorMock.Object,
            MockInternshipQueryFactory.Create().Object,
db);

        var request = new AddProjectMemberRequest(mentorId, MemberRole.Member);

        // Act
        var result = await handler.Handle(
            new AddProjectMemberCommand(projectId, request), CancellationToken.None);

        // Assert
        result.UserId.Should().Be(mentorId);
        result.Role.Should().Be(nameof(MemberRole.Member));

        var member = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == mentorId);
        member.Should().NotBeNull();
        member!.RemovedAt.Should().BeNull();
    }

    [Fact]
    public async Task AddMember_AdminAddsIntern_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var internId = await ProjectTestSetup.SeedInternAsync(db);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new AddProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            mediatorMock.Object,
            MockInternshipQueryFactory.Create(inBatch: true).Object,
            db);

        var request = new AddProjectMemberRequest(internId, MemberRole.Member);

        // Act
        var result = await handler.Handle(
            new AddProjectMemberCommand(projectId, request), CancellationToken.None);

        // Assert
        result.UserId.Should().Be(internId);
    }

    [Fact]
    public async Task AddMember_TargetUserNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new AddProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            mediatorMock.Object,
            MockInternshipQueryFactory.Create().Object,
db);

        var request = new AddProjectMemberRequest(Guid.NewGuid(), MemberRole.Member);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new AddProjectMemberCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task AddMember_InternNotInBatch_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var internId = await ProjectTestSetup.SeedInternAsync(db);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new AddProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            mediatorMock.Object,
            MockInternshipQueryFactory.Create(inBatch: false).Object,
            db);

        var request = new AddProjectMemberRequest(internId, MemberRole.Member);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new AddProjectMemberCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task AddMember_MentorAddsIntern_AssignedToHim_Success()
    {
        // Arrange — Mentor (Lead) thêm intern mình phụ trách
        await using var db = TestDbContextFactory.Create();
        var (_, _, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var leadId = await ProjectTestSetup.SeedMentorAsync(db);
        var internId = await ProjectTestSetup.SeedInternAsync(db);

        db.ProjectMembers.Add(EntityFactory.CreateLead(projectId, leadId));
        await db.SaveChangesAsync();

        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new AddProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(leadId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object,
            MockInternshipQueryFactory.Create(
                inBatch: true, mentorOfIntern: true).Object,
                db);

        var request = new AddProjectMemberRequest(internId, MemberRole.Member);

        // Act
        var result = await handler.Handle(
            new AddProjectMemberCommand(projectId, request), CancellationToken.None);

        // Assert
        result.UserId.Should().Be(internId);
    }

    [Fact]
    public async Task AddMember_MentorAddsIntern_NotAssignedToHim_ThrowsForbidden()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var leadId = await ProjectTestSetup.SeedMentorAsync(db);
        var internId = await ProjectTestSetup.SeedInternAsync(db);

        db.ProjectMembers.Add(EntityFactory.CreateLead(projectId, leadId));
        await db.SaveChangesAsync();

        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new AddProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(leadId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object,
            MockInternshipQueryFactory.Create(
                inBatch: true, mentorOfIntern: false).Object,
                db);

        var request = new AddProjectMemberRequest(internId, MemberRole.Member);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new AddProjectMemberCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task AddMember_MentorAddsMentor_ThrowsForbidden()
    {
        // Arrange — Mentor không được thêm Mentor khác (chỉ Admin)
        await using var db = TestDbContextFactory.Create();
        var (_, _, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var leadId = await ProjectTestSetup.SeedMentorAsync(db);
        var otherMentorId = await ProjectTestSetup.SeedMentorAsync(db);

        db.ProjectMembers.Add(EntityFactory.CreateLead(projectId, leadId));
        await db.SaveChangesAsync();

        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new AddProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(leadId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object,
            MockInternshipQueryFactory.Create().Object,
db);

        var request = new AddProjectMemberRequest(otherMentorId, MemberRole.Member);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new AddProjectMemberCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task AddMember_DuplicateActive_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var mentorId = await ProjectTestSetup.SeedMentorAsync(db);

        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = mentorId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new AddProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            mediatorMock.Object,
            MockInternshipQueryFactory.Create().Object,
db);

        var request = new AddProjectMemberRequest(mentorId, MemberRole.Member);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new AddProjectMemberCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task AddMember_PreviouslyRemoved_Reactivates()
    {
        // Arrange — Member từng bị xóa mềm → reactivate, không báo lỗi
        await using var db = TestDbContextFactory.Create();
        var (_, adminId, projectId) = await ProjectTestSetup
            .SetupProjectWithAdminAsync(db);
        var mentorId = await ProjectTestSetup.SeedMentorAsync(db);

        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = mentorId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow.AddDays(-10),
            RemovedAt = DateTime.UtcNow.AddDays(-5)
        });
        await db.SaveChangesAsync();

        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new AddProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            mediatorMock.Object,
            MockInternshipQueryFactory.Create().Object,
db);

        var request = new AddProjectMemberRequest(mentorId, MemberRole.Lead);

        // Act
        var result = await handler.Handle(
            new AddProjectMemberCommand(projectId, request), CancellationToken.None);

        // Assert
        result.UserId.Should().Be(mentorId);

        var member = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == mentorId);
        member.Should().NotBeNull();
        member!.RemovedAt.Should().BeNull();
        member.Role.Should().Be(MemberRole.Lead);
    }
}

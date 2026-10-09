using FluentAssertions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Projects.Commands.AddProjectMember;
using InternFlow.Application.Features.Projects.Commands.RemoveProjectMember;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Application.Features.Projects.Queries.GetProjectMembers;
using TaskStatus = InternFlow.Domain.Projects.TaskStatus;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InternFlow.Tests.Features.Projects;

public class ProjectMemberTests
{

    private static Mock<IInternshipQueryService> MockInternshipQuery(
        bool inBatch = true,
        bool mentorOfIntern = true)
    {
        var m = new Mock<IInternshipQueryService>();
        m.Setup(x => x.IsInternInBatchAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inBatch);
        m.Setup(x => x.IsMentorAssignedToInternAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mentorOfIntern);
        return m;
    }


    [Fact]
    public async Task AddMember_AdminRole_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var mentorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));
        db.Users.Add(new User
        {
            Id = mentorId,
            Email = "mentor@test.com",
            FullName = "Mentor",
            Role = UserRole.Mentor,
            PasswordHash = "hash",
            IsActive = true,
            RowVersion = Array.Empty<byte>()
        });
        var project = EntityFactory.CreateProject(internshipId, adminId);
        project.Id = projectId;
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new AddProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            mediatorMock.Object,
            MockInternshipQuery().Object)
            ;

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
    public async Task AddMember_InternNotInBatch_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var internId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));
        db.Users.Add(new User
        {
            Id = internId,
            Email = "intern@test.com",
            FullName = "Intern",
            Role = UserRole.Intern,
            PasswordHash = "hash",
            IsActive = true,
            RowVersion = Array.Empty<byte>()
        });
        var project = EntityFactory.CreateProject(internshipId, adminId);
        project.Id = projectId;
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new AddProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            mediatorMock.Object,
            MockInternshipQuery(inBatch: false).Object);

        var request = new AddProjectMemberRequest(internId, MemberRole.Member);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new AddProjectMemberCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task AddMember_DuplicateActive_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var mentorId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));
        db.Users.Add(new User
        {
            Id = mentorId,
            Email = "m@t.com",
            FullName = "M",
            Role = UserRole.Mentor,
            PasswordHash = "h",
            IsActive = true,
            RowVersion = Array.Empty<byte>()
        });
        var project = EntityFactory.CreateProject(internshipId, adminId);
        project.Id = projectId;
        db.Projects.Add(project);
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
            MockInternshipQuery().Object);

        var request = new AddProjectMemberRequest(mentorId, MemberRole.Member);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new AddProjectMemberCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task AddMember_MentorNotAssignedToIntern_ThrowsForbidden()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var otherMentorId = Guid.NewGuid();
        var internId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, leadId));
        db.Users.AddRange(
            new User
            {
                Id = leadId,
                Email = "lead@t.com",
                FullName = "Lead",
                Role = UserRole.Mentor,
                PasswordHash = "h",
                IsActive = true,
                RowVersion = Array.Empty<byte>()
            },
            new User
            {
                Id = internId,
                Email = "i@t.com",
                FullName = "I",
                Role = UserRole.Intern,
                PasswordHash = "h",
                IsActive = true,
                RowVersion = Array.Empty<byte>()
            });

        var project = EntityFactory.CreateProject(internshipId, leadId);
        project.Id = projectId;
        db.Projects.Add(project);

        // Lead là Lead của project
        db.ProjectMembers.Add(EntityFactory.CreateLead(projectId, leadId));
        await db.SaveChangesAsync();
        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new AddProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(leadId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object,
            MockInternshipQuery(inBatch: true, mentorOfIntern: false).Object);

        var request = new AddProjectMemberRequest(internId, MemberRole.Member);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new AddProjectMemberCommand(projectId, request),
                CancellationToken.None));
    }
    [Fact]
    public async Task RemoveMember_AdminRole_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));
        var project = EntityFactory.CreateProject(internshipId, adminId);
        project.Id = projectId;
        db.Projects.Add(project);
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

        // Assert
        var member = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == memberId);
        member.Should().NotBeNull();
        member!.RemovedAt.Should().NotBeNull();   // soft delete
    }

    [Fact]
    public async Task RemoveMember_LastLead_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));
        var project = EntityFactory.CreateProject(internshipId, adminId);
        project.Id = projectId;
        db.Projects.Add(project);
        db.ProjectMembers.Add(EntityFactory.CreateLead(projectId, leadId));
        await db.SaveChangesAsync();
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new RemoveProjectMemberCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            mediatorMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new RemoveProjectMemberCommand(projectId, leadId),
                CancellationToken.None));
    }

    [Fact]
    public async Task RemoveMember_HasOpenTasks_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));
        var project = EntityFactory.CreateProject(internshipId, adminId);
        project.Id = projectId;
        db.Projects.Add(project);
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
            handler.Handle(new RemoveProjectMemberCommand(projectId, memberId),
                CancellationToken.None));
    }

    // ═══════════════════════════════════════════════════════
    // GET MEMBERS
    // ═══════════════════════════════════════════════════════

    [Fact]
    public async Task GetMembers_ReturnsOnlyActive()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var activeId = Guid.NewGuid();
        var removedId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));
        db.Users.AddRange(
            new User
            {
                Id = activeId,
                Email = "a@t.com",
                FullName = "Active",
                Role = UserRole.Mentor,
                PasswordHash = "h",
                IsActive = true,
                RowVersion = Array.Empty<byte>()
            },
            new User
            {
                Id = removedId,
                Email = "r@t.com",
                FullName = "Removed",
                Role = UserRole.Mentor,
                PasswordHash = "h",
                IsActive = true,
                RowVersion = Array.Empty<byte>()
            });
        var project = EntityFactory.CreateProject(internshipId, adminId);
        project.Id = projectId;
        db.Projects.Add(project);

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

        var result = await handler.Handle(
            new GetProjectMembersQuery(projectId), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].UserId.Should().Be(activeId);
    }
}

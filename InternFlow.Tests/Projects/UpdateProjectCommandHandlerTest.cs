using FluentAssertions;
using InternFlow.Application.Features.Projects.Commands.UpdateProject;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InternFlow.Tests.Features.Projects;

public class UpdateProjectCommandHandlerTests
{
    [Fact]
    public async Task Update_AdminRole_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));

        var project = EntityFactory.CreateProject(internshipId, adminId);
        project.Id = projectId;
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var handler = new UpdateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance);

        var request = new UpdateProjectRequest(
            Name: "Payment API v2",
            Description: "Updated",
            Status: ProjectStatus.Active,
            StartDate: new DateOnly(2026, 10, 1),
            EndDate: new DateOnly(2026, 12, 31));

        // Act
        var result = await handler.Handle(
            new UpdateProjectCommand(projectId, request), CancellationToken.None);

        // Assert
        result.Name.Should().Be("Payment API v2");
        result.Status.Should().Be(ProjectStatus.Active.ToString());
        result.Description.Should().Be("Updated");

        // Verify DB
        var updated = await db.Projects.FindAsync(projectId);
        updated!.Name.Should().Be("Payment API v2");
        updated.Status.Should().Be(ProjectStatus.Active);
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_LeadRole_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var mentorId = Guid.NewGuid();
        var leadId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, mentorId));

        var project = EntityFactory.CreateProject(internshipId, mentorId);
        project.Id = projectId;
        db.Projects.Add(project);
        db.ProjectMembers.Add(EntityFactory.CreateLead(projectId, leadId));
        await db.SaveChangesAsync();

        var handler = new UpdateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Create(leadId, UserRole.Mentor),   // ← Mentor = Lead
            TestMapperFactory.Instance);

        var request = new UpdateProjectRequest(
            "Updated Name", null, ProjectStatus.Active, null, null);

        // Act
        var result = await handler.Handle(
            new UpdateProjectCommand(projectId, request), CancellationToken.None);

        // Assert
        result.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Update_MemberRole_ThrowsForbidden()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var mentorId = Guid.NewGuid();
        var memberId = Guid.NewGuid();   // không phải Lead
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, mentorId));

        var project = EntityFactory.CreateProject(internshipId, mentorId);
        project.Id = projectId;
        db.Projects.Add(project);

        // Member, không phải Lead
        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = memberId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = new UpdateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Create(memberId, UserRole.Mentor),
            TestMapperFactory.Instance);

        var request = new UpdateProjectRequest(
            "Updated", null, ProjectStatus.Active, null, null);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new UpdateProjectCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Update_ProjectNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var adminId = Guid.NewGuid();

        var handler = new UpdateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance);

        var request = new UpdateProjectRequest(
            "X", null, ProjectStatus.Active, null, null);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new UpdateProjectCommand(Guid.NewGuid(), request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Update_DuplicateName_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));

        var p1 = EntityFactory.CreateProject(internshipId, adminId, "Project A");
        p1.Id = projectId;
        var p2 = EntityFactory.CreateProject(internshipId, adminId, "Project B");
        db.Projects.AddRange(p1, p2);
        await db.SaveChangesAsync();

        var handler = new UpdateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance);

        // Đổi Project A → tên "Project B" (đã tồn tại)
        var request = new UpdateProjectRequest(
            "Project B", null, ProjectStatus.Active, null, null);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new UpdateProjectCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Update_SameName_NoConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));

        var project = EntityFactory.CreateProject(internshipId, adminId, "Project A");
        project.Id = projectId;
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var handler = new UpdateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance);

        // Đổi description, giữ nguyên tên
        var request = new UpdateProjectRequest(
            "Project A", "New description", ProjectStatus.Active, null, null);

        // Act
        var result = await handler.Handle(
            new UpdateProjectCommand(projectId, request), CancellationToken.None);

        // Assert
        result.Name.Should().Be("Project A");
        result.Description.Should().Be("New description");
    }
}

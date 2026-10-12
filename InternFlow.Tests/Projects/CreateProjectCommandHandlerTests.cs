using FluentAssertions;
using InternFlow.Application.Features.Projects.Commands.CreateProject;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InternFlow.Tests.Projects;

/// <summary>
/// Test cho CreateProjectCommandHandler.
/// Bao gồm: happy path, internship inactive, duplicate name, phân quyền.
/// </summary>
public class CreateProjectCommandHandlerTests
{
    [Fact]
    public async Task Create_ValidInternship_Success_CreatorBecomesLead()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));
        await db.SaveChangesAsync();

        var handler = new CreateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance, db);

        var request = new CreateProjectRequest(
            internshipId, "Payment API", "Desc", null, null);

        // Act
        var result = await handler.Handle(
            new CreateProjectCommand(request), CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Payment API");
        result.CreatedBy.Should().Be(adminId);
        result.Status.Should().Be(ProjectStatus.Planned.ToString());

        // Người tạo → Lead
        var lead = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == result.Id);
        lead.Should().NotBeNull();
        lead!.UserId.Should().Be(adminId);
        lead.Role.Should().Be(MemberRole.Lead);
        lead.RemovedAt.Should().BeNull();
    }

    [Fact]
    public async Task Create_MentorRole_Success()
    {
        // Arrange — Mentor cũng được tạo project (EnsureRole cho phép Admin + Mentor)
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var mentorId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, mentorId));
        await db.SaveChangesAsync();

        var handler = new CreateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
db);

        var request = new CreateProjectRequest(
            internshipId, "Mentor Project", null, null, null);

        // Act
        var result = await handler.Handle(
            new CreateProjectCommand(request), CancellationToken.None);

        // Assert
        result.CreatedBy.Should().Be(mentorId);

        var lead = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == result.Id);
        lead!.UserId.Should().Be(mentorId);
        lead.Role.Should().Be(MemberRole.Lead);
    }

    [Fact]
    public async Task Create_InactiveInternship_ThrowsNotFound()
    {
        // Arrange — Internship Closed, không Active
        await using var db = TestDbContextFactory.Create();
        var adminId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(
            status: InternshipStatus.Closed, createdBy: adminId));
        await db.SaveChangesAsync();

        var handler = new CreateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
db);

        var request = new CreateProjectRequest(
            Guid.NewGuid(), "Test", null, null, null);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CreateProjectCommand(request), CancellationToken.None));
    }

    [Fact]
    public async Task Create_InternshipNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var adminId = Guid.NewGuid();

        var handler = new CreateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
db);

        var request = new CreateProjectRequest(
            Guid.NewGuid(), "Test", null, null, null);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CreateProjectCommand(request), CancellationToken.None));
    }

    [Fact]
    public async Task Create_DuplicateName_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));
        db.Projects.Add(EntityFactory.CreateProject(
            internshipId, adminId, "Payment API"));
        await db.SaveChangesAsync();

        var handler = new CreateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
db);

        var request = new CreateProjectRequest(
            internshipId, "Payment API", null, null, null);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateProjectCommand(request), CancellationToken.None));
    }

    [Fact]
    public async Task Create_InternRole_ThrowsForbidden()
    {
        // Arrange — Intern không được tạo project
        await using var db = TestDbContextFactory.Create();
        var internId = Guid.NewGuid();

        var handler = new CreateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
db);

        var request = new CreateProjectRequest(
            Guid.NewGuid(), "X", null, null, null);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new CreateProjectCommand(request), CancellationToken.None));
    }

    [Fact]
    public async Task Create_Unauthenticated_ThrowsForbidden()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();

        var handler = new CreateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Anonymous(),
            TestMapperFactory.Instance,
db);

        var request = new CreateProjectRequest(
            Guid.NewGuid(), "X", null, null, null);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new CreateProjectCommand(request), CancellationToken.None));
    }
}

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

namespace InternFlow.Tests.Features.Projects;

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
            TestMapperFactory.Instance);

        var request = new CreateProjectRequest(
            internshipId, "Payment API", "Desc", null, null);

        // Act
        var result = await handler.Handle(
            new CreateProjectCommand(request), CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Payment API");
        result.CreatedBy.Should().Be(adminId);

        // Người tạo → Lead
        var lead = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == result.Id);
        lead.Should().NotBeNull();
        lead!.UserId.Should().Be(adminId);
        lead.Role.Should().Be(MemberRole.Lead);
        lead.RemovedAt.Should().BeNull();
    }

    [Fact]
    public async Task Create_InactiveInternship_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var adminId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(
            status: InternshipStatus.Closed, createdBy: adminId));
        await db.SaveChangesAsync();

        var handler = new CreateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance);

        var request = new CreateProjectRequest(
            Guid.NewGuid(), "Test", null, null, null);   // internshipId sai

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
            TestMapperFactory.Instance);

        var request = new CreateProjectRequest(
            internshipId, "Payment API", null, null, null);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateProjectCommand(request), CancellationToken.None));
    }

    [Fact]
    public async Task Create_InternRole_ThrowsForbidden()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var internId = Guid.NewGuid();

        var handler = new CreateProjectCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance);

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
            TestMapperFactory.Instance);

        var request = new CreateProjectRequest(
            Guid.NewGuid(), "X", null, null, null);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new CreateProjectCommand(request), CancellationToken.None));
    }
}

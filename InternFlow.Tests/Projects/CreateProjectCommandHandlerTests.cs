using AutoMapper;
using FluentAssertions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Projects.Commands.CreateProject;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;
using InternFlow.Domain.Projects;
using InternFlow.Infrastructure.Data;
using InternFlow.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace InternFlow.Tests.Features.Projects;

public class CreateProjectCommandHandlerTests
{
    private static InternFlowDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<InternFlowDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new InternFlowDbContext(options);
    }

    private static ICurrentUserService MockUser(Guid id, UserRole role)
    {
        var m = new Mock<ICurrentUserService>();
        m.Setup(x => x.UserId).Returns(id);
        m.Setup(x => x.Role).Returns(role.ToString());
        m.Setup(x => x.IsAuthenticated).Returns(true);
        m.Setup(x => x.IsInRole(It.IsAny<string>()))
            .Returns<string>(r => string.Equals(r, role.ToString(),
                StringComparison.OrdinalIgnoreCase));
        return m.Object;
    }

    [Fact]
    public async Task Create_ValidInternship_Success()
    {
        var db = CreateDb();
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        db.Internships.Add(new Internship
        {
            Id = internshipId,
            Name = "Summer 2026",
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 9, 30),
            Status = InternshipStatus.Active,
            CreatedBy = adminId
        });
        await db.SaveChangesAsync();
        var internshipSvc = new Mock<IInternshipQueryService>();
        var handler = new CreateProjectCommandHandler(
            db, MockUser(adminId, UserRole.Admin));

        var request = new CreateProjectRequest(
            internshipId, "Payment API", "Desc", null, null);

        var result = await handler.Handle(new CreateProjectCommand(request), default);

        result.Id.Should().NotBeEmpty();
        result.Status.Should().Be("Planned");
        result.CreatedBy.Should().Be(adminId);

        // Lead created
        var lead = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == result.Id);
        lead.Should().NotBeNull();
        lead!.Role.Should().Be(MemberRole.Lead);
    }

    [Fact]
    public async Task Create_InactiveInternship_ThrowsNotFound()
    {
        var db = CreateDb();
        var handler = new CreateProjectCommandHandler(
            db, MockUser(Guid.NewGuid(), UserRole.Admin)
            );

        var request = new CreateProjectRequest(
            Guid.NewGuid(), "Test", null, null, null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CreateProjectCommand(request), default));
    }

    [Fact]
    public async Task Create_DuplicateName_ThrowsConflict()
    {
        var db = CreateDb();
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        db.Internships.Add(new Internship
        {
            Id = internshipId,
            Name = "S2026",
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 9, 30),
            Status = InternshipStatus.Active,
            CreatedBy = adminId
        });
        db.Projects.Add(new Project
        {
            Id = Guid.NewGuid(),
            InternshipId = internshipId,
            Name = "Payment API",
            Status = ProjectStatus.Planned,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        var handler = new CreateProjectCommandHandler(
            db, MockUser(adminId, UserRole.Admin)
            );

        var request = new CreateProjectRequest(
            internshipId, "Payment API", null, null, null);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateProjectCommand(request), default));
    }

    [Fact]
    public async Task Create_InternRole_ThrowsForbidden()
    {
        var db = CreateDb();
        var handler = new CreateProjectCommandHandler(
            db, MockUser(Guid.NewGuid(), UserRole.Intern));

        var request = new CreateProjectRequest(
            Guid.NewGuid(), "X", null, null, null);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new CreateProjectCommand(request), default));
    }
}

using FluentAssertions;
using InternFlow.Domain.Projects;
using InternFlow.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InternFlow.Tests.Projects;

public class ProjectAndMemberSqlServerTests : SqlServerTestBase
{
    [Fact]
    public async Task AddDuplicateProjectName_InSameInternship_ShouldThrowDbUpdateException()
    {
        // Arrange
        var internshipId = Guid.NewGuid();

        var project1 = new Project
        {
            Id = Guid.NewGuid(),
            InternshipId = internshipId,
            Name = "InternFlow Core",
            CreatedBy = Guid.NewGuid()
        };

        var project2 = new Project
        {
            Id = Guid.NewGuid(),
            InternshipId = internshipId, // Trùng đợt
            Name = "InternFlow Core",       // Trùng tên
            CreatedBy = Guid.NewGuid()
        };

        Context.Projects.Add(project1);
        await Context.SaveChangesAsync();

        // Act
        Context.Projects.Add(project2);
        Func<Task> act = async () => await Context.SaveChangesAsync();

        // Assert: Ném lỗi từ SQL Server thật do vướng Unique Index UQ_Projects_Internship_Name
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task AddActiveMember_TwiceToSameProject_ShouldThrowDbUpdateException()
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            InternshipId = Guid.NewGuid(),
            Name = "Project For Member Test",
            CreatedBy = Guid.NewGuid()
        };

        Context.Projects.Add(project);
        await Context.SaveChangesAsync();

        var userId = Guid.NewGuid();

        var member1 = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            UserId = userId,
            MemberRole = "Member",
            RemovedAt = null
        };

        var member2 = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id, // Trùng ProjectId
            UserId = userId,        // Trùng UserId
            MemberRole = "Member",
            RemovedAt = null        // Cùng Active
        };

        Context.ProjectMembers.Add(member1);
        await Context.SaveChangesAsync();

        Context.ProjectMembers.Add(member2);
        Func<Task> act = async () => await Context.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }
}

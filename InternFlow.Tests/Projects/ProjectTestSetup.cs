using InternFlow.Domain.Identity;
using InternFlow.Infrastructure.Data;
using InternFlow.Tests.Common.TestHelpers;

namespace InternFlow.Tests.Projects;

/// <summary>
/// Helper seed data dùng chung cho mọi Project test.
/// </summary>
internal static class ProjectTestSetup
{
    /// <summary>
    /// Seed Internship (Active) + Admin + Project.
    /// Admin là Lead của project.
    /// </summary>
    public static async Task<(Guid InternshipId, Guid AdminId, Guid ProjectId)>
        SetupProjectWithAdminAsync(InternFlowDbContext db)
    {
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));
        db.Users.Add(EntityFactory.CreateUser(adminId, UserRole.Admin));

        var project = EntityFactory.CreateProject(internshipId, adminId);
        project.Id = projectId;
        db.Projects.Add(project);

        db.ProjectMembers.Add(EntityFactory.CreateLead(projectId, adminId));

        await db.SaveChangesAsync();
        return (internshipId, adminId, projectId);
    }

    /// <summary>Seed Mentor chưa là member.</summary>
    public static async Task<Guid> SeedMentorAsync(InternFlowDbContext db)
    {
        var mentorId = Guid.NewGuid();
        db.Users.Add(EntityFactory.CreateUser(mentorId, UserRole.Mentor));
        await db.SaveChangesAsync();
        return mentorId;
    }

    /// <summary>Seed Intern chưa là member.</summary>
    public static async Task<Guid> SeedInternAsync(InternFlowDbContext db)
    {
        var internId = Guid.NewGuid();
        db.Users.Add(EntityFactory.CreateUser(internId, UserRole.Intern));
        await db.SaveChangesAsync();
        return internId;
    }
}

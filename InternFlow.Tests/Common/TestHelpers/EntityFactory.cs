using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;
using InternFlow.Domain.Projects;

namespace InternFlow.Tests.Common.TestHelpers;

public static class EntityFactory
{
    public static Internship CreateInternship(
        Guid? id = null,
        Guid? createdBy = null,
        InternshipStatus status = InternshipStatus.Active)
    {
        return new Internship
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Summer 2026",
            StartDate = new DateOnly(2026, 6, 1),
            EndDate = new DateOnly(2026, 9, 30),
            Status = status,
            CreatedBy = createdBy ?? Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static Project CreateProject(
        Guid internshipId,
        Guid createdBy,
        string name = "Payment API",
        ProjectStatus status = ProjectStatus.Planned)
    {
        return new Project
        {
            Id = Guid.NewGuid(),
            InternshipId = internshipId,
            Name = name,
            Description = "Test project",
            Status = status,
            StartDate = null,
            EndDate = null,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static ProjectMember CreateLead(Guid projectId, Guid userId)
    {
        return new ProjectMember
        {
            ProjectId = projectId,
            UserId = userId,
            Role = MemberRole.Lead,
            JoinedAt = DateTime.UtcNow
        };
    }
    public static User CreateUser(
        Guid id,
        UserRole role,
        string? email = null,
        string? fullName = null,
        bool isActive = true)
    {
        return new User
        {
            Id = id,
            Email = email ?? $"{role.ToString().ToLower()}@test.com",
            FullName = fullName ?? role.ToString(),
            Role = role,
            PasswordHash = "hash",
            IsActive = isActive,
            RowVersion = Array.Empty<byte>()
        };
    }
}

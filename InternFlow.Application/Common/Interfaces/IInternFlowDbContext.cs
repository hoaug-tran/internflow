using InternFlow.Domain.Evaluations;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;
using InternFlow.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Common.Interfaces;

public interface IInternFlowDbContext
{
    DbSet<User> Users { get; }
    DbSet<InternProfile> InternProfiles { get; }
    DbSet<MentorProfile> MentorProfiles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Internship> Internships { get; }
    DbSet<BatchMentor> BatchMentors { get; }
    DbSet<BatchIntern> BatchInterns { get; }
    DbSet<MentorAssignment> MentorAssignments { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DbSet<TaskItem> Tasks { get; }
    DbSet<WorkLog> WorkLogs { get; }

    DbSet<EvaluationPeriod> EvaluationPeriods { get; }
    DbSet<Criterion> Criteria { get; }
    DbSet<Evaluation> Evaluations { get; }
    DbSet<EvaluationDetail> EvaluationDetails { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

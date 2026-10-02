using InternFlow.Domain.Entities;
using InternFlow.Domain.Entities.Views;
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
    DbSet<CurrentMentorAssignmentView> CurrentMentorAssignments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

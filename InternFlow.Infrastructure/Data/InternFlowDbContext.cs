using InternFlow.Domain.Entities;
using InternFlow.Domain.Entities.Views;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Infrastructure.Data;

public class InternFlowDbContext : DbContext
{
    public InternFlowDbContext(DbContextOptions<InternFlowDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<InternProfile> InternProfiles => Set<InternProfile>();
    public DbSet<MentorProfile> MentorProfiles => Set<MentorProfile>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Internship> Internships => Set<Internship>();
    public DbSet<BatchMentor> BatchMentors => Set<BatchMentor>();
    public DbSet<BatchIntern> BatchInterns => Set<BatchIntern>();
    public DbSet<MentorAssignment> MentorAssignments => Set<MentorAssignment>();
    public DbSet<CurrentMentorAssignmentView> CurrentMentorAssignments => Set<CurrentMentorAssignmentView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tự động quét và nạp toàn bộ cấu hình IEntityTypeConfiguration trong assembly Infrastructure
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InternFlowDbContext).Assembly);
    }
}

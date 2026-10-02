using InternFlow.Domain.Evaluations;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Infrastructure.Data;

public static class DevSeeder
{
    // Khớp FakeCurrentUserService
    public static readonly Guid MentorId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid AdminId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid InternshipId = Guid.Parse("11111111-0000-0000-0000-000000000000");
    private static readonly Guid PeriodId = Guid.Parse("e1111111-1111-1111-1111-000000000003");

    public static async Task SeedAsync(InternFlowDbContext db, CancellationToken ct = default)
    {
        if (await db.Internships.AnyAsync(ct)) return;

        db.Users.AddRange(
            new User { Id = AdminId, Email = "admin@internflow.com", PasswordHash = "dev-only", FullName = "Admin Demo", Role = UserRole.Admin },
            new User { Id = MentorId, Email = "mentor.demo@internflow.com", PasswordHash = "dev-only", FullName = "Mentor Demo", Role = UserRole.Mentor });

        db.Internships.Add(new Internship
        {
            Id = InternshipId,
            Name = "Đợt thực tập demo",
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 12, 1),
            Status = InternshipStatus.Active,
            CreatedBy = AdminId
        });

        db.EvaluationPeriods.Add(new EvaluationPeriod
        {
            Id = PeriodId,
            InternshipId = InternshipId,
            Name = "Đánh giá giữa kỳ",
            StartDate = new DateOnly(2026, 10, 1),
            EndDate = new DateOnly(2026, 10, 11),
            Status = PeriodStatus.Open
        });

        var criteria = new (string Name, decimal Weight)[]
        {
            ("Thái độ làm việc và kỷ luật", 20),
            ("Kỹ năng lập trình", 30),
            ("Khả năng làm việc nhóm", 20),
            ("Tiến độ hoàn thành Task", 20),
            ("Khả năng chủ động học hỏi", 10)
        };

        db.Criteria.AddRange(criteria.Select((c, i) => new Criterion
        {
            EvaluationPeriodId = PeriodId,
            Name = c.Name,
            Weight = c.Weight,
            DisplayOrder = i + 1
        }));

        await db.SaveChangesAsync(ct);
    }
}

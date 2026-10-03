using InternFlow.Domain.Evaluations;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Infrastructure.Data;

public static class DevSeeder
{
    // Khớp FakeCurrentUserService
    public static readonly Guid MentorId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static readonly Guid AdminId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    public static readonly Guid InternId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid InternshipId = Guid.Parse("11111111-0000-0000-0000-000000000000");
    public static readonly Guid PeriodId = Guid.Parse("e1111111-1111-1111-1111-000000000003");

    public static async Task SeedAsync(InternFlowDbContext db, CancellationToken ct = default)
    {
        if (await db.Internships.AnyAsync(ct)) return;

        // Băm mật khẩu chuẩn BCrypt cho tài khoản demo ("Password123!")
        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");

        var adminUser = new User
        {
            Id = AdminId,
            Email = "admin@internflow.com",
            PasswordHash = defaultPasswordHash,
            FullName = "Admin Demo",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var mentorUser = new User
        {
            Id = MentorId,
            Email = "mentor.demo@internflow.com",
            PasswordHash = defaultPasswordHash,
            FullName = "Mentor Demo",
            Role = UserRole.Mentor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            MentorProfile = new MentorProfile
            {
                UserId = MentorId,
                UserRole = UserRole.Mentor,
                Department = "Phát triển phần mềm",
                JobTitle = "Senior Backend Engineer",
                Expertise = "C# .NET, SQL Server",
                Phone = "0987654321",
                MaxMentees = 5,
                CreatedAt = DateTime.UtcNow
            }
        };

        var internUser = new User
        {
            Id = InternId,
            Email = "intern.demo@internflow.com",
            PasswordHash = defaultPasswordHash,
            FullName = "Intern Demo",
            Role = UserRole.Intern,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            InternProfile = new InternProfile
            {
                UserId = InternId,
                UserRole = UserRole.Intern,
                StudentCode = "SV2026001",
                University = "Đại học Bách Khoa",
                Major = "Công nghệ Thông tin",
                Phone = "0123456789",
                CreatedAt = DateTime.UtcNow
            }
        };

        db.Users.AddRange(adminUser, mentorUser, internUser);

        var internship = new Internship
        {
            Id = InternshipId,
            Name = "Đợt thực tập demo",
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 12, 1),
            Status = InternshipStatus.Active,
            CreatedBy = AdminId,
            CreatedAt = DateTime.UtcNow
        };
        db.Internships.Add(internship);

        db.BatchMentors.Add(new BatchMentor
        {
            InternshipId = InternshipId,
            MentorId = MentorId,
            MentorRole = UserRole.Mentor,
            AddedBy = AdminId,
            JoinedAt = DateTime.UtcNow
        });

        db.BatchInterns.Add(new BatchIntern
        {
            InternshipId = InternshipId,
            InternId = InternId,
            InternRole = UserRole.Intern,
            Status = BatchInternStatus.InProgress,
            AddedBy = AdminId,
            JoinedAt = DateTime.UtcNow
        });

        db.MentorAssignments.Add(new MentorAssignment
        {
            InternshipId = InternshipId,
            InternId = InternId,
            MentorId = MentorId,
            AssignedBy = AdminId,
            AssignedAt = DateTime.UtcNow
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

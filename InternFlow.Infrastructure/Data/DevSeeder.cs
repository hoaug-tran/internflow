using InternFlow.Domain.Evaluations;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;
using InternFlow.Domain.Projects;
using TaskStatus = InternFlow.Domain.Projects.TaskStatus;
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
    public static readonly Guid ProjectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid Task1Id = Guid.Parse("44444444-4444-4444-4444-000000000001");
    public static readonly Guid Task2Id = Guid.Parse("44444444-4444-4444-4444-000000000002");
    public static readonly Guid Task3Id = Guid.Parse("44444444-4444-4444-4444-000000000003");

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
        // ─── Project ────────────────────────────────────────────────────────────
        var projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var project = new Project
        {
            Id = projectId,
            InternshipId = InternshipId,
            Name = "Dự án Demo InternFlow",
            Description = "Dự án mẫu để test cache",
            Status = ProjectStatus.Active,
            StartDate = new DateOnly(2026, 10, 1),
            EndDate = new DateOnly(2026, 12, 1),
            CreatedBy = MentorId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Projects.Add(project);

        // ─── ProjectMembers ─────────────────────────────────────────────────────
        // Mentor là Lead, Intern là Member
        db.ProjectMembers.AddRange(
            new ProjectMember
            {
                ProjectId = projectId,
                UserId = MentorId,
                Role = MemberRole.Lead,
                JoinedAt = DateTime.UtcNow
            },
            new ProjectMember
            {
                ProjectId = projectId,
                UserId = InternId,
                Role = MemberRole.Member,
                JoinedAt = DateTime.UtcNow
            });

        // ─── Tasks ──────────────────────────────────────────────────────────────
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var taskIds = new[]
        {
    Guid.Parse("44444444-4444-4444-4444-000000000001"),
    Guid.Parse("44444444-4444-4444-4444-000000000002"),
    Guid.Parse("44444444-4444-4444-4444-000000000003")
};

        db.Tasks.AddRange(
            new TaskItem
            {
                Id = taskIds[0],
                ProjectId = projectId,
                AssigneeId = InternId,
                Title = "Task 1 — Todo",
                Status = TaskStatus.Todo,
                Priority = TaskPriority.Medium,
                DueDate = today.AddDays(7),
                CreatedBy = MentorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = taskIds[1],
                ProjectId = projectId,
                AssigneeId = InternId,
                Title = "Task 2 — InProgress",
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                DueDate = today.AddDays(3),
                CreatedBy = MentorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = taskIds[2],
                ProjectId = projectId,
                AssigneeId = InternId,
                Title = "Task 3 — Done",
                Status = TaskStatus.Done,
                Priority = TaskPriority.Low,
                DueDate = today.AddDays(-1),
                CompletedAt = DateTime.UtcNow.AddDays(-1),
                CreatedBy = MentorId,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            });

        await db.SaveChangesAsync(ct);
    }
}

using InternFlow.Domain.Enums;

namespace InternFlow.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public int FailedLoginCount { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Concurrency Token: EF Core dùng byte[] để map với ROWVERSION trong SQL Server
    public byte[] RowVersion { get; set; } = null!;

    // === Navigation Properties ===
    public InternProfile? InternProfile { get; set; }
    public MentorProfile? MentorProfile { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    // Đợt thực tập do User này tạo (FK_Batches_CreatedBy)
    public ICollection<Internship> CreatedInternships { get; set; } = new List<Internship>();

    // Tham gia với vai trò Mentor trong đợt (FK_BM_Mentor)
    public ICollection<BatchMentor> BatchMentors { get; set; } = new List<BatchMentor>();

    // Tham gia với vai trò Intern trong đợt (FK_BI_Intern)
    public ICollection<BatchIntern> BatchInterns { get; set; } = new List<BatchIntern>();

    // Các thành viên đợt do User này thêm vào (FK_BM_AddedBy, FK_BI_AddedBy)
    public ICollection<BatchMentor> AddedBatchMentors { get; set; } = new List<BatchMentor>();
    public ICollection<BatchIntern> AddedBatchInterns { get; set; } = new List<BatchIntern>();

    // Các phân công do User này thực hiện (FK_MA_AssignedBy)
    public ICollection<MentorAssignment> AssignedMentorAssignments { get; set; } = new List<MentorAssignment>();
}

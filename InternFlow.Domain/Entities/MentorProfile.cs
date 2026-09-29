using InternFlow.Domain.Enums;

namespace InternFlow.Domain.Entities;

public class MentorProfile
{
    public Guid UserId { get; set; }
    public UserRole UserRole { get; set; } = UserRole.Mentor;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? Expertise { get; set; }
    public string? Phone { get; set; }
    public int? MaxMentees { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation trỏ ngược lại User
    public User User { get; set; } = null!;
}
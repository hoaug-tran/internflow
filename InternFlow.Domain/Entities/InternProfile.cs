using InternFlow.Domain.Enums;

namespace InternFlow.Domain.Entities;

public class InternProfile
{
    public Guid UserId { get; set; }
    public UserRole UserRole { get; set; } = UserRole.Intern;
    public string? StudentCode { get; set; }
    public string? University { get; set; }
    public string? Major { get; set; }
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? CvUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation trỏ ngược lại User
    public User User { get; set; } = null!;
}

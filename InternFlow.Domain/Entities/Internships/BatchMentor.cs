using InternFlow.Domain.Enums;

namespace InternFlow.Domain.Entities;

public class BatchMentor
{
    public Guid BatchId { get; set; }
    public Guid MentorId { get; set; }
    public UserRole MentorRole { get; set; } = UserRole.Mentor;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public Guid AddedBy { get; set; }

    // === Navigation Properties ===
    public Internship Batch { get; set; } = null!;
    public User Mentor { get; set; } = null!;
    public User AddedByUser { get; set; } = null!;
    public ICollection<MentorAssignment> MentorAssignments { get; set; } = new List<MentorAssignment>();
}

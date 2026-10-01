using InternFlow.Domain.Identity;

namespace InternFlow.Domain.Internships;

public class BatchIntern
{
    public Guid InternshipId { get; set; }
    public Guid InternId { get; set; }
    public UserRole InternRole { get; set; } = UserRole.Intern;
    public BatchInternStatus Status { get; set; } = BatchInternStatus.InProgress;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public Guid AddedBy { get; set; }
    public string? Notes { get; set; }

    public Internship Internship { get; set; } = null!;
    public User Intern { get; set; } = null!;
    public User AddedByUser { get; set; } = null!;
    public ICollection<MentorAssignment> MentorAssignments { get; set; } = new List<MentorAssignment>();
}

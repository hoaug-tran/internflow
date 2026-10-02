using InternFlow.Domain.Identity;

namespace InternFlow.Domain.Internships;

public class BatchMentor
{
    public Guid InternshipId { get; set; }
    public Guid MentorId { get; set; }
    public UserRole MentorRole { get; set; } = UserRole.Mentor;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public Guid AddedBy { get; set; }

    public Internship Internship { get; set; } = null!;
    public User Mentor { get; set; } = null!;
    public User AddedByUser { get; set; } = null!;
    public ICollection<MentorAssignment> MentorAssignments { get; set; } = new List<MentorAssignment>();
}

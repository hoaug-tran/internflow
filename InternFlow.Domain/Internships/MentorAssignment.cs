using InternFlow.Domain.Identity;

namespace InternFlow.Domain.Internships;

public class MentorAssignment
{
    private bool? _isCurrent;

    public long Id { get; set; }
    public Guid InternshipId { get; set; }
    public Guid InternId { get; set; }
    public Guid MentorId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid AssignedBy { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? EndReason { get; set; }
    public bool IsCurrent
    {
        get => _isCurrent ?? EndedAt is null;
        private set => _isCurrent = value;
    }

    public BatchIntern BatchIntern { get; set; } = null!;
    public BatchMentor BatchMentor { get; set; } = null!;
    public Internship Internship { get; set; } = null!;
    public User AssignedByUser { get; set; } = null!;
}

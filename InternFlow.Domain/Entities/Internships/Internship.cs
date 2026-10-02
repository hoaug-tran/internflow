using InternFlow.Domain.Enums;

namespace InternFlow.Domain.Entities;

public class Internship
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public BatchStatus Status { get; set; } = BatchStatus.Planned;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // === Navigation Properties ===
    public User Creator { get; set; } = null!;
    public ICollection<BatchMentor> BatchMentors { get; set; } = new List<BatchMentor>();
    public ICollection<BatchIntern> BatchInterns { get; set; } = new List<BatchIntern>();
    public ICollection<MentorAssignment> MentorAssignments { get; set; } = new List<MentorAssignment>();
}

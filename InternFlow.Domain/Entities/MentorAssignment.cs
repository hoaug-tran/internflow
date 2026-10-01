namespace InternFlow.Domain.Entities;

public class MentorAssignment
{
    private bool? _isCurrent;

    public long Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid InternId { get; set; }
    public Guid MentorId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid AssignedBy { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? EndReason { get; set; }

    /// <summary>
    /// Thuộc tính tính toán (Computed Column trong DB: CASE WHEN EndedAt IS NULL THEN 1 ELSE 0 END).
    /// </summary>
    public bool IsCurrent
    {
        get => _isCurrent ?? (EndedAt == null);
        private set => _isCurrent = value;
    }

    // === Navigation Properties ===
    public BatchIntern BatchIntern { get; set; } = null!;
    public BatchMentor BatchMentor { get; set; } = null!;
    public Internship Batch { get; set; } = null!;
    public User AssignedByUser { get; set; } = null!;
}

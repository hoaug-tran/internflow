namespace InternFlow.Domain.Entities.Views;

/// <summary>
/// Entity biểu diễn Database View dbo.vw_CurrentMentorAssignments (Keyless Entity Type)
/// </summary>
public class CurrentMentorAssignmentView
{
    public long AssignmentId { get; set; }
    public Guid BatchId { get; set; }
    public Guid InternId { get; set; }
    public Guid MentorId { get; set; }
    public DateTime AssignedAt { get; set; }
}

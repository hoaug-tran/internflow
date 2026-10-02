namespace InternFlow.Infrastructure.Data.ReadModels;

public class CurrentMentorAssignmentView
{
    public long AssignmentId { get; set; }
    public Guid BatchId { get; set; }
    public Guid InternId { get; set; }
    public Guid MentorId { get; set; }
    public DateTime AssignedAt { get; set; }
}

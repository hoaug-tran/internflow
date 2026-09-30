namespace InternFlow.Domain.Projects;

public class ProjectMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string MemberRole { get; set; } = "Member";
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RemovedAt { get; set; }

    public Project Project { get; set; } = null!;
}
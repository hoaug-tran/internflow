using InternFlow.Domain.Identity;

namespace InternFlow.Domain.Projects;

public class ProjectMember
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RemovedAt { get; set; }

    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}

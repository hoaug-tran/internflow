using InternFlow.Domain.Common;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;

namespace InternFlow.Domain.Projects;

public class Project : AuditableEntity
{
    public Guid InternshipId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid CreatedBy { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Internship Internship { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}

using InternFlow.Domain.Common;
using InternFlow.Domain.Identity;

namespace InternFlow.Domain.Projects;

public class TaskItem : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public Guid? AssigneeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateOnly? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Project Project { get; set; } = null!;
    public User? Assignee { get; set; }
    public ICollection<WorkLog> WorkLogs { get; set; } = new List<WorkLog>();
}

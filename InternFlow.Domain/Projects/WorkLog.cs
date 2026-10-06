using InternFlow.Domain.Common;
using InternFlow.Domain.Identity;

namespace InternFlow.Domain.Projects;

public class WorkLog : AuditableEntity
{
    public Guid TaskItemId { get; set; }
    public Guid UserId { get; set; }
    public DateOnly WorkDate { get; set; }
    public decimal Hours { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public TaskItem TaskItem { get; set; } = null!;
    public User User { get; set; } = null!;
}

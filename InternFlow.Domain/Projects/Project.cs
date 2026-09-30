namespace InternFlow.Domain.Projects;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InternshipId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Active";
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
}

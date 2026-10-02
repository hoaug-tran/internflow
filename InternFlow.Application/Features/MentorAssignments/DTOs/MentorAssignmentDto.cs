namespace InternFlow.Application.Features.MentorAssignments.DTOs;

public class MentorAssignmentDto
{
    public long Id { get; set; }
    public Guid InternshipId { get; set; }
    public string InternshipName { get; set; } = string.Empty;
    public Guid InternId { get; set; }
    public string InternName { get; set; } = string.Empty;
    public string InternEmail { get; set; } = string.Empty;
    public Guid MentorId { get; set; }
    public string MentorName { get; set; } = string.Empty;
    public string MentorEmail { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public Guid AssignedBy { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? EndReason { get; set; }
    public bool IsCurrent { get; set; }
}

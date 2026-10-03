using InternFlow.Domain.Internships;

namespace InternFlow.Application.Features.Internships.DTOs;

public class BatchInternDto
{
    public Guid InternId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? StudentCode { get; set; }
    public string? University { get; set; }
    public string? Major { get; set; }
    public BatchInternStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime JoinedAt { get; set; }
    public string? Notes { get; set; }
    public Guid? CurrentMentorId { get; set; }
    public string? CurrentMentorName { get; set; }
}

namespace InternFlow.Application.Features.Internships.DTOs;

public class BatchMentorDto
{
    public Guid MentorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public int? MaxMentees { get; set; }
    public int CurrentMenteeCount { get; set; }
    public DateTime JoinedAt { get; set; }
}

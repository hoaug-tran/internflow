using InternFlow.Domain.Internships;

namespace InternFlow.Application.Features.Internships.DTOs;

public class InternshipDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public InternshipStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MentorCount { get; set; }
    public int InternCount { get; set; }
}

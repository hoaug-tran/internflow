using InternFlow.Domain.Internships;

namespace InternFlow.Application.Features.Internships.DTOs;

public class InternshipDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public InternshipStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public Guid CreatedBy { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<BatchMentorDto> Mentors { get; set; } = new();
    public List<BatchInternDto> Interns { get; set; } = new();
}

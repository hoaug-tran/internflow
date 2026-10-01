using InternFlow.Domain.Common;
using InternFlow.Domain.Evaluations;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;

namespace InternFlow.Domain.Internships;

public class Internship : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public InternshipStatus Status { get; set; } = InternshipStatus.Planned;
    public Guid CreatedBy { get; set; }

    public User Creator { get; set; } = null!;
    public ICollection<BatchMentor> BatchMentors { get; set; } = new List<BatchMentor>();
    public ICollection<BatchIntern> BatchInterns { get; set; } = new List<BatchIntern>();
    public ICollection<MentorAssignment> MentorAssignments { get; set; } = new List<MentorAssignment>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<EvaluationPeriod> EvaluationPeriods { get; set; } = new List<EvaluationPeriod>();
}

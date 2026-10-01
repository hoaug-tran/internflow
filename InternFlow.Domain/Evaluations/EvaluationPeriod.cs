using InternFlow.Domain.Common;
using InternFlow.Domain.Internships;

namespace InternFlow.Domain.Evaluations;

public class EvaluationPeriod : AuditableEntity
{
    public Guid InternshipId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public PeriodStatus Status { get; set; } = PeriodStatus.Draft;

    public Internship Internship { get; set; } = null!;
    public ICollection<Criterion> Criteria { get; set; } = new List<Criterion>();
    public ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
}

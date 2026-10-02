using InternFlow.Domain.Common;
using InternFlow.Domain.Identity;

namespace InternFlow.Domain.Evaluations;

public class Evaluation : AuditableEntity
{
    public Guid EvaluationPeriodId { get; set; }
    public Guid InternId { get; set; }
    public Guid EvaluatorId { get; set; }
    public EvaluationStatus Status { get; set; } = EvaluationStatus.Draft;
    public string? OverallComment { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public EvaluationPeriod EvaluationPeriod { get; set; } = null!;
    public User Intern { get; set; } = null!;
    public User Evaluator { get; set; } = null!;
    public ICollection<EvaluationDetail> Details { get; set; } = new List<EvaluationDetail>();
}

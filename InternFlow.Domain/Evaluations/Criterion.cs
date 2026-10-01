using InternFlow.Domain.Common;

namespace InternFlow.Domain.Evaluations;

public class Criterion : AuditableEntity
{
    public Guid EvaluationPeriodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Weight { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public EvaluationPeriod EvaluationPeriod { get; set; } = null!;
    public ICollection<EvaluationDetail> EvaluationDetails { get; set; } = new List<EvaluationDetail>();
}

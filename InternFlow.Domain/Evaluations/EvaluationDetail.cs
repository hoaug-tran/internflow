using InternFlow.Domain.Common;

namespace InternFlow.Domain.Evaluations;

public class EvaluationDetail : BaseEntity
{
    public Guid EvaluationId { get; set; }
    public Guid CriterionId { get; set; }
    public decimal Score { get; set; }
    public string? Comment { get; set; }

    public Evaluation Evaluation { get; set; } = null!;
    public Criterion Criterion { get; set; } = null!;
}

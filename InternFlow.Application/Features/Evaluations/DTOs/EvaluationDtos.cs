namespace InternFlow.Application.Features.Evaluations.DTOs;

public class EvaluationPeriodDto
{
    public Guid Id { get; set; }
    public Guid InternshipId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CriterionDto
{
    public Guid Id { get; set; }
    public Guid EvaluationPeriodId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Weight { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class ScoreInput
{
    public Guid CriterionId { get; set; }
    public decimal Score { get; set; }
    public string? Comment { get; set; }
}

public class EvaluationDetailDto
{
    public Guid CriterionId { get; set; }
    public string CriterionName { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Score { get; set; }
    public string? Comment { get; set; }
}

public class EvaluationDto
{
    public Guid Id { get; set; }
    public Guid EvaluationPeriodId { get; set; }
    public Guid InternId { get; set; }
    public Guid EvaluatorId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? OverallComment { get; set; }
    public decimal TotalScore { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public List<EvaluationDetailDto> Details { get; set; } = new();
}

using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Domain.Evaluations;

namespace InternFlow.Application.Features.Evaluations;

public static class EvaluationMapping
{
    public const decimal MaxTotalWeight = 100m;
    public static string CriteriaCacheKey(Guid periodId) => $"criteria:{periodId}";
    public static readonly TimeSpan CriteriaTtl = TimeSpan.FromMinutes(10);

    public static EvaluationPeriodDto ToDto(this EvaluationPeriod p) => new()
    {
        Id = p.Id,
        InternshipId = p.InternshipId,
        Name = p.Name,
        Description = p.Description,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Status = p.Status.ToString()
    };

    public static CriterionDto ToDto(this Criterion c) => new()
    {
        Id = c.Id,
        EvaluationPeriodId = c.EvaluationPeriodId,
        Name = c.Name,
        Description = c.Description,
        Weight = c.Weight,
        DisplayOrder = c.DisplayOrder,
        IsActive = c.IsActive
    };
    public static EvaluationDto ToDto(this Evaluation e, IReadOnlyCollection<Criterion> criteria)
    {
        var map = criteria.ToDictionary(c => c.Id);
        return new EvaluationDto
        {
            Id = e.Id,
            EvaluationPeriodId = e.EvaluationPeriodId,
            InternId = e.InternId,
            EvaluatorId = e.EvaluatorId,
            Status = e.Status.ToString(),
            OverallComment = e.OverallComment,
            SubmittedAt = e.SubmittedAt,
            TotalScore = e.CalculateTotal(criteria),
            Details = e.Details
                .Where(d => map.ContainsKey(d.CriterionId))
                .OrderBy(d => map[d.CriterionId].DisplayOrder)
                .Select(d => new EvaluationDetailDto
                {
                    CriterionId = d.CriterionId,
                    CriterionName = map[d.CriterionId].Name,
                    Weight = map[d.CriterionId].Weight,
                    Score = d.Score,
                    Comment = d.Comment
                }).ToList()
        };
    }
}

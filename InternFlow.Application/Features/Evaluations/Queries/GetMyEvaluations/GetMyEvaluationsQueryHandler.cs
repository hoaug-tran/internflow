using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Domain.Evaluations;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Queries.GetMyEvaluations;

public class GetMyEvaluationsQueryHandler(
    IEvaluationRepository evaluations,
    ICriterionRepository criteria,
    ICurrentUserService currentUser)
    : IRequestHandler<GetMyEvaluationsQuery, ApiResponse<List<MyEvaluationSummaryDto>>>
{
    public async Task<ApiResponse<List<MyEvaluationSummaryDto>>> Handle(GetMyEvaluationsQuery r, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenException("Không xác định được người dùng hiện tại.");

        var items = await evaluations.GetPublishedByInternAsync(userId, ct);

        var periodIds = items.Select(e => e.EvaluationPeriodId).Distinct().ToList();
        var activeCriteria = await criteria.GetActiveByPeriodsAsync(periodIds, ct);
        var byPeriod = activeCriteria.ToLookup(c => c.EvaluationPeriodId);

        var list = items.Select(e => new MyEvaluationSummaryDto
        {
            Id = e.Id,
            EvaluationPeriodId = e.EvaluationPeriodId,
            PeriodName = e.EvaluationPeriod.Name,
            TotalScore = e.CalculateTotal(byPeriod[e.EvaluationPeriodId]),
            SubmittedAt = e.SubmittedAt
        }).ToList();

        return ApiResponse<List<MyEvaluationSummaryDto>>.Ok(list);
    }
}

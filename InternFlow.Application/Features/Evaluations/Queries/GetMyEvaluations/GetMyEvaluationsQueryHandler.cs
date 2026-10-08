using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Domain.Evaluations;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Evaluations.Queries.GetMyEvaluations;

public class GetMyEvaluationsQueryHandler(IInternFlowDbContext context, ICurrentUserService currentUser)
    : IRequestHandler<GetMyEvaluationsQuery, ApiResponse<List<MyEvaluationSummaryDto>>>
{
    public async Task<ApiResponse<List<MyEvaluationSummaryDto>>> Handle(GetMyEvaluationsQuery r, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenException("Không xác định được người dùng hiện tại.");

        var evaluations = await context.Evaluations.AsNoTracking()
            .Include(e => e.Details)
            .Include(e => e.EvaluationPeriod)
            .Where(e => e.InternId == userId && e.Status == EvaluationStatus.Published)
            .OrderByDescending(e => e.SubmittedAt)
            .ToListAsync(ct);

        var periodIds = evaluations.Select(e => e.EvaluationPeriodId).Distinct().ToList();
        var criteria = await context.Criteria.AsNoTracking()
            .Where(c => periodIds.Contains(c.EvaluationPeriodId) && c.IsActive)
            .ToListAsync(ct);
        var byPeriod = criteria.ToLookup(c => c.EvaluationPeriodId);

        var list = evaluations.Select(e => new MyEvaluationSummaryDto
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

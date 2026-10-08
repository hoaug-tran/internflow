using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Evaluations.Queries.GetPeriods;

public class GetPeriodsQueryHandler(IInternFlowDbContext context)
    : IRequestHandler<GetPeriodsQuery, ApiResponse<List<EvaluationPeriodDto>>>
{
    public async Task<ApiResponse<List<EvaluationPeriodDto>>> Handle(GetPeriodsQuery r, CancellationToken ct)
    {
        var q = context.EvaluationPeriods.AsNoTracking();
        if (r.InternshipId.HasValue) q = q.Where(p => p.InternshipId == r.InternshipId.Value);

        var list = (await q.OrderByDescending(p => p.StartDate).ToListAsync(ct)).Select(p => p.ToDto()).ToList();
        return ApiResponse<List<EvaluationPeriodDto>>.Ok(list);
    }
}

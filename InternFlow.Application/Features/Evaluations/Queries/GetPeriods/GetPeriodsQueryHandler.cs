using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Queries.GetPeriods;

public class GetPeriodsQueryHandler(IEvaluationPeriodRepository periods)
    : IRequestHandler<GetPeriodsQuery, ApiResponse<List<EvaluationPeriodDto>>>
{
    public async Task<ApiResponse<List<EvaluationPeriodDto>>> Handle(GetPeriodsQuery r, CancellationToken ct)
    {
        var list = (await periods.GetListAsync(r.InternshipId, ct)).Select(p => p.ToDto()).ToList();
        return ApiResponse<List<EvaluationPeriodDto>>.Ok(list);
    }
}

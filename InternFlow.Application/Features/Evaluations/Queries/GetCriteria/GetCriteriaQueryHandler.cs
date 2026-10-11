using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Queries.GetCriteria;

public class GetCriteriaQueryHandler(
    IEvaluationPeriodRepository periods,
    ICriterionRepository criteria,
    ICacheService cache)
    : IRequestHandler<GetCriteriaQuery, ApiResponse<List<CriterionDto>>>
{
    public async Task<ApiResponse<List<CriterionDto>>> Handle(GetCriteriaQuery r, CancellationToken ct)
    {
        var key = EvaluationMapping.CriteriaCacheKey(r.PeriodId);

        var cached = await cache.GetAsync<List<CriterionDto>>(key, ct);
        if (cached is not null) return ApiResponse<List<CriterionDto>>.Ok(cached);

        if (!await periods.ExistsAsync(r.PeriodId, ct))
            throw new NotFoundException("Không tìm thấy kỳ đánh giá.");

        var list = (await criteria.GetByPeriodAsync(r.PeriodId, ct))
            .Select(c => c.ToDto()).ToList();

        await cache.SetAsync(key, list, EvaluationMapping.CriteriaTtl, ct);
        return ApiResponse<List<CriterionDto>>.Ok(list);
    }
}

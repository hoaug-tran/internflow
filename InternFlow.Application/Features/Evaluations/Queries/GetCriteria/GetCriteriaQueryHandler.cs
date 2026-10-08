using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Evaluations.Queries.GetCriteria;

public class GetCriteriaQueryHandler(IInternFlowDbContext context, ICacheService cache)
    : IRequestHandler<GetCriteriaQuery, ApiResponse<List<CriterionDto>>>
{
    public async Task<ApiResponse<List<CriterionDto>>> Handle(GetCriteriaQuery r, CancellationToken ct)
    {
        var key = EvaluationMapping.CriteriaCacheKey(r.PeriodId);

        var cached = await cache.GetAsync<List<CriterionDto>>(key, ct);
        if (cached is not null) return ApiResponse<List<CriterionDto>>.Ok(cached);

        if (!await context.EvaluationPeriods.AnyAsync(p => p.Id == r.PeriodId, ct))
            throw new NotFoundException("Không tìm thấy kỳ đánh giá.");

        var list = (await context.Criteria.AsNoTracking()
                .Where(c => c.EvaluationPeriodId == r.PeriodId)
                .OrderBy(c => c.DisplayOrder).ToListAsync(ct))
            .Select(c => c.ToDto()).ToList();

        await cache.SetAsync(key, list, EvaluationMapping.CriteriaTtl, ct);
        return ApiResponse<List<CriterionDto>>.Ok(list);
    }
}

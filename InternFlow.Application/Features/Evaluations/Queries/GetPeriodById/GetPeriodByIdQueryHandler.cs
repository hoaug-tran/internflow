using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Queries.GetPeriodById;

public class GetPeriodByIdQueryHandler(IEvaluationPeriodRepository periods)
    : IRequestHandler<GetPeriodByIdQuery, ApiResponse<EvaluationPeriodDto>>
{
    public async Task<ApiResponse<EvaluationPeriodDto>> Handle(GetPeriodByIdQuery r, CancellationToken ct)
    {
        var p = await periods.GetByIdNoTrackingAsync(r.Id, ct)
            ?? throw new NotFoundException("Không tìm thấy kỳ đánh giá.");
        return ApiResponse<EvaluationPeriodDto>.Ok(p.ToDto());
    }
}

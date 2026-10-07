using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Evaluations.Queries.GetPeriodById;

public class GetPeriodByIdQueryHandler(IInternFlowDbContext context)
    : IRequestHandler<GetPeriodByIdQuery, ApiResponse<EvaluationPeriodDto>>
{
    public async Task<ApiResponse<EvaluationPeriodDto>> Handle(GetPeriodByIdQuery r, CancellationToken ct)
    {
        var p = await context.EvaluationPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == r.Id, ct)
            ?? throw new NotFoundException("Không tìm thấy kỳ đánh giá.");
        return ApiResponse<EvaluationPeriodDto>.Ok(p.ToDto());
    }
}

using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Domain.Evaluations;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Evaluations.Queries.GetMyEvaluationById;

public class GetMyEvaluationByIdQueryHandler(IInternFlowDbContext context, ICurrentUserService currentUser)
    : IRequestHandler<GetMyEvaluationByIdQuery, ApiResponse<EvaluationDto>>
{
    public async Task<ApiResponse<EvaluationDto>> Handle(GetMyEvaluationByIdQuery r, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenException("Không xác định được người dùng hiện tại.");

        // Không tồn tại, Draft, hoặc của người khác đều cùng một lỗi 404
        var evaluation = await context.Evaluations.AsNoTracking()
                .Include(e => e.Details)
                .FirstOrDefaultAsync(e => e.Id == r.Id && e.InternId == userId
                                          && e.Status == EvaluationStatus.Published, ct)
            ?? throw new NotFoundException("Không tìm thấy phiếu đánh giá.");

        var criteria = await EvaluationGuard.LoadActiveCriteriaAsync(context, evaluation.EvaluationPeriodId, ct);

        return ApiResponse<EvaluationDto>.Ok(evaluation.ToDto(criteria));
    }
}

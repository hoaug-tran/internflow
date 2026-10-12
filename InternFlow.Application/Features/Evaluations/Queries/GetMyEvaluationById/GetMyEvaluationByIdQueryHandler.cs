using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Queries.GetMyEvaluationById;

public class GetMyEvaluationByIdQueryHandler(
    IEvaluationRepository evaluations,
    ICriterionRepository criteria,
    ICurrentUserService currentUser)
    : IRequestHandler<GetMyEvaluationByIdQuery, ApiResponse<EvaluationDto>>
{
    public async Task<ApiResponse<EvaluationDto>> Handle(GetMyEvaluationByIdQuery r, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenException("Không xác định được người dùng hiện tại.");

        // Không tồn tại, Draft, hoặc của người khác đều cùng một lỗi 404
        var evaluation = await evaluations.GetPublishedByIdForInternAsync(r.Id, userId, ct)
            ?? throw new NotFoundException("Không tìm thấy phiếu đánh giá.");

        var activeCriteria = await criteria.GetActiveByPeriodAsync(evaluation.EvaluationPeriodId, ct);

        return ApiResponse<EvaluationDto>.Ok(evaluation.ToDto(activeCriteria));
    }
}

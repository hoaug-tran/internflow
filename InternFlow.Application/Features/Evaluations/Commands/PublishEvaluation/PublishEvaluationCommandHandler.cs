using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Domain.Evaluations;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.PublishEvaluation;

public class PublishEvaluationCommandHandler(
    IEvaluationPeriodRepository periods,
    ICriterionRepository criteria,
    IEvaluationRepository evaluations,
    IInternshipQueryService internships,
    ICurrentUserService currentUser,
    IUnitOfWork uow)
    : IRequestHandler<PublishEvaluationCommand, ApiResponse<EvaluationDto>>
{
    public async Task<ApiResponse<EvaluationDto>> Handle(PublishEvaluationCommand r, CancellationToken ct)
    {
        var evaluation = await evaluations.GetByIdWithDetailsAsync(r.Id, ct)
            ?? throw new NotFoundException("Không tìm thấy phiếu đánh giá.");

        var period = await periods.GetByIdNoTrackingAsync(evaluation.EvaluationPeriodId, ct)
            ?? throw new NotFoundException("Không tìm thấy kỳ đánh giá.");

        await EvaluationGuard.EnsureCanEvaluateAsync(internships, currentUser, period.InternshipId, evaluation.InternId, ct);
        evaluation.EnsureEditable();
        EvaluationGuard.EnsurePeriodOpen(period);

        var activeCriteria = await criteria.GetActiveByPeriodAsync(period.Id, ct);

        evaluation.Publish(activeCriteria);
        await uow.SaveChangesAsync(ct);

        return ApiResponse<EvaluationDto>.Ok(evaluation.ToDto(activeCriteria), "Publish phiếu đánh giá thành công");
    }
}

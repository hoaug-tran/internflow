using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Domain.Evaluations;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.UpdateEvaluation;

public class UpdateEvaluationCommandHandler(
    IEvaluationPeriodRepository periods,
    ICriterionRepository criteria,
    IEvaluationRepository evaluations,
    IInternshipQueryService internships,
    ICurrentUserService currentUser,
    IUnitOfWork uow)
    : IRequestHandler<UpdateEvaluationCommand, ApiResponse<EvaluationDto>>
{
    public async Task<ApiResponse<EvaluationDto>> Handle(UpdateEvaluationCommand r, CancellationToken ct)
    {
        var evaluation = await evaluations.GetByIdWithDetailsAsync(r.Id, ct)
            ?? throw new NotFoundException("Không tìm thấy phiếu đánh giá.");

        var period = await periods.GetByIdNoTrackingAsync(evaluation.EvaluationPeriodId, ct)
            ?? throw new NotFoundException("Không tìm thấy kỳ đánh giá.");

        await EvaluationGuard.EnsureCanEvaluateAsync(internships, currentUser, period.InternshipId, evaluation.InternId, ct);
        evaluation.EnsureEditable();
        EvaluationGuard.EnsurePeriodOpen(period);

        var activeCriteria = await criteria.GetActiveByPeriodAsync(period.Id, ct);

        var added = evaluation.SetScores(activeCriteria,
            r.Scores.Select(s => new ScoreEntry(s.CriterionId, s.Score, s.Comment)));
        evaluation.OverallComment = r.OverallComment;

        // Detail mới có Id sinh sẵn nên phải Add tường minh, nếu không EF coi là Modified
        evaluations.AddDetails(added);
        await uow.SaveChangesAsync(ct);

        return ApiResponse<EvaluationDto>.Ok(evaluation.ToDto(activeCriteria), "Cập nhật phiếu đánh giá thành công");
    }
}

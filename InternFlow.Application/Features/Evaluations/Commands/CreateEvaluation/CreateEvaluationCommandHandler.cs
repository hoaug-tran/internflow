using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Domain.Evaluations;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.CreateEvaluation;

public class CreateEvaluationCommandHandler(
    IEvaluationPeriodRepository periods,
    ICriterionRepository criteria,
    IEvaluationRepository evaluations,
    IInternshipQueryService internships,
    ICurrentUserService currentUser,
    IUnitOfWork uow)
    : IRequestHandler<CreateEvaluationCommand, ApiResponse<EvaluationDto>>
{
    public async Task<ApiResponse<EvaluationDto>> Handle(CreateEvaluationCommand r, CancellationToken ct)
    {
        var evaluatorId = currentUser.UserId
            ?? throw new ForbiddenException("Không xác định được người dùng hiện tại.");

        var period = await periods.GetByIdNoTrackingAsync(r.PeriodId, ct)
            ?? throw new NotFoundException("Không tìm thấy kỳ đánh giá.");

        EvaluationGuard.EnsurePeriodOpen(period);
        await EvaluationGuard.EnsureCanEvaluateAsync(internships, currentUser, period.InternshipId, r.InternId, ct);

        if (await evaluations.ExistsAsync(r.PeriodId, r.InternId, ct))
            throw new ConflictException("Intern này đã có phiếu đánh giá trong kỳ.");

        var activeCriteria = await criteria.GetActiveByPeriodAsync(r.PeriodId, ct);

        var evaluation = new Evaluation
        {
            EvaluationPeriodId = r.PeriodId,
            InternId = r.InternId,
            EvaluatorId = evaluatorId,
            OverallComment = r.OverallComment
        };
        evaluation.SetScores(activeCriteria, r.Scores.Select(s => new ScoreEntry(s.CriterionId, s.Score, s.Comment)));

        evaluations.Add(evaluation);
        await uow.SaveChangesAsync(ct);

        return ApiResponse<EvaluationDto>.Ok(evaluation.ToDto(activeCriteria), "Tạo phiếu đánh giá thành công");
    }
}

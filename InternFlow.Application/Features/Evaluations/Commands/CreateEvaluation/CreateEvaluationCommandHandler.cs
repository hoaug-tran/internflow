using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Domain.Evaluations;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Evaluations.Commands.CreateEvaluation;

public class CreateEvaluationCommandHandler(IInternFlowDbContext context, ICurrentUserService currentUser)
    : IRequestHandler<CreateEvaluationCommand, ApiResponse<EvaluationDto>>
{
    public async Task<ApiResponse<EvaluationDto>> Handle(CreateEvaluationCommand r, CancellationToken ct)
    {
        var evaluatorId = currentUser.UserId
            ?? throw new ForbiddenException("Không xác định được người dùng hiện tại.");

        var period = await context.EvaluationPeriods.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == r.PeriodId, ct)
            ?? throw new NotFoundException("Không tìm thấy kỳ đánh giá.");

        EvaluationGuard.EnsurePeriodOpen(period);
        await EvaluationGuard.EnsureCanEvaluateAsync(context, currentUser, period.InternshipId, r.InternId, ct);

        if (await context.Evaluations.AnyAsync(e => e.EvaluationPeriodId == r.PeriodId && e.InternId == r.InternId, ct))
            throw new ConflictException("Intern này đã có phiếu đánh giá trong kỳ.");

        var criteria = await EvaluationGuard.LoadActiveCriteriaAsync(context, r.PeriodId, ct);

        var evaluation = new Evaluation
        {
            EvaluationPeriodId = r.PeriodId,
            InternId = r.InternId,
            EvaluatorId = evaluatorId,
            OverallComment = r.OverallComment
        };
        evaluation.SetScores(criteria, r.Scores.Select(s => new ScoreEntry(s.CriterionId, s.Score, s.Comment)));

        context.Evaluations.Add(evaluation);
        await context.SaveChangesAsync(ct);

        return ApiResponse<EvaluationDto>.Ok(evaluation.ToDto(criteria), "Tạo phiếu đánh giá thành công");
    }
}

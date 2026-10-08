using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Domain.Evaluations;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Evaluations.Commands.UpdateEvaluation;

public class UpdateEvaluationCommandHandler(IInternFlowDbContext context, ICurrentUserService currentUser)
    : IRequestHandler<UpdateEvaluationCommand, ApiResponse<EvaluationDto>>
{
    public async Task<ApiResponse<EvaluationDto>> Handle(UpdateEvaluationCommand r, CancellationToken ct)
    {
        var evaluation = await context.Evaluations.Include(e => e.Details)
                .FirstOrDefaultAsync(e => e.Id == r.Id, ct)
            ?? throw new NotFoundException("Không tìm thấy phiếu đánh giá.");

        var period = await context.EvaluationPeriods.AsNoTracking()
            .FirstAsync(p => p.Id == evaluation.EvaluationPeriodId, ct);

        await EvaluationGuard.EnsureCanEvaluateAsync(context, currentUser, period.InternshipId, evaluation.InternId, ct);
        evaluation.EnsureEditable();
        EvaluationGuard.EnsurePeriodOpen(period);

        var criteria = await EvaluationGuard.LoadActiveCriteriaAsync(context, period.Id, ct);

        var added = evaluation.SetScores(criteria,
            r.Scores.Select(s => new ScoreEntry(s.CriterionId, s.Score, s.Comment)));
        evaluation.OverallComment = r.OverallComment;

        // Detail mới có Id sinh sẵn nên phải Add tường minh, nếu không EF coi là Modified
        context.EvaluationDetails.AddRange(added);
        await context.SaveChangesAsync(ct);

        return ApiResponse<EvaluationDto>.Ok(evaluation.ToDto(criteria), "Cập nhật phiếu đánh giá thành công");
    }
}

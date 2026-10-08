using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using InternFlow.Domain.Evaluations;

namespace InternFlow.Application.Features.Evaluations.Commands.PublishEvaluation;

public class PublishEvaluationCommandHandler(IInternFlowDbContext context, ICurrentUserService currentUser)
    : IRequestHandler<PublishEvaluationCommand, ApiResponse<EvaluationDto>>
{
    public async Task<ApiResponse<EvaluationDto>> Handle(PublishEvaluationCommand r, CancellationToken ct)
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

        evaluation.Publish(criteria);
        await context.SaveChangesAsync(ct);

        return ApiResponse<EvaluationDto>.Ok(evaluation.ToDto(criteria), "Publish phiếu đánh giá thành công");
    }
}

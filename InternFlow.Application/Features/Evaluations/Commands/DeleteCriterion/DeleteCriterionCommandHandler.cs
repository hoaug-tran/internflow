using InternFlow.Application.Common.Interfaces;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Evaluations.Commands.DeleteCriterion;

public class DeleteCriterionCommandHandler(IInternFlowDbContext context, ICacheService cache)
    : IRequestHandler<DeleteCriterionCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(DeleteCriterionCommand r, CancellationToken ct)
    {
        var c = await context.Criteria.FirstOrDefaultAsync(x => x.Id == r.Id && x.EvaluationPeriodId == r.PeriodId, ct)
            ?? throw new NotFoundException("Không tìm thấy tiêu chí.");

        if (await context.EvaluationDetails.AnyAsync(d => d.CriterionId == r.Id, ct))
            throw new ConflictException("Tiêu chí đã được dùng trong phiếu đánh giá, không thể xóa.");

        context.Criteria.Remove(c);
        await context.SaveChangesAsync(ct);
        await cache.RemoveAsync(EvaluationMapping.CriteriaCacheKey(r.PeriodId), ct);
        return ApiResponse<Guid>.Ok(r.Id, "Xóa tiêu chí thành công");
    }
}

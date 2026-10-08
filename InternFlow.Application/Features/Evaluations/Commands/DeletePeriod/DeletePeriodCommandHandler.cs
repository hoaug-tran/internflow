using InternFlow.Application.Common.Interfaces;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Evaluations.Commands.DeletePeriod;

public class DeletePeriodCommandHandler(IInternFlowDbContext context, ICacheService cache)
    : IRequestHandler<DeletePeriodCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(DeletePeriodCommand r, CancellationToken ct)
    {
        var p = await context.EvaluationPeriods.Include(x => x.Criteria).FirstOrDefaultAsync(x => x.Id == r.Id, ct)
            ?? throw new NotFoundException("Không tìm thấy kỳ đánh giá.");

        if (await context.Evaluations.AnyAsync(e => e.EvaluationPeriodId == r.Id, ct))
            throw new ConflictException("Kỳ đánh giá đã có phiếu đánh giá, không thể xóa.");

        context.Criteria.RemoveRange(p.Criteria);
        context.EvaluationPeriods.Remove(p);
        await context.SaveChangesAsync(ct);
        await cache.RemoveAsync(EvaluationMapping.CriteriaCacheKey(r.Id), ct);
        return ApiResponse<Guid>.Ok(r.Id, "Xóa kỳ đánh giá thành công");
    }
}

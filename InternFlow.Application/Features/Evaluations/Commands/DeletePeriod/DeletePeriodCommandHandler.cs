using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.DeletePeriod;

public class DeletePeriodCommandHandler(
    IEvaluationPeriodRepository periods,
    ICriterionRepository criteria,
    IEvaluationRepository evaluations,
    IUnitOfWork uow,
    ICacheService cache)
    : IRequestHandler<DeletePeriodCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(DeletePeriodCommand r, CancellationToken ct)
    {
        var p = await periods.GetByIdWithCriteriaAsync(r.Id, ct)
            ?? throw new NotFoundException("Không tìm thấy kỳ đánh giá.");

        if (await evaluations.AnyInPeriodAsync(r.Id, ct))
            throw new ConflictException("Kỳ đánh giá đã có phiếu đánh giá, không thể xóa.");

        criteria.RemoveRange(p.Criteria);
        periods.Remove(p);
        await uow.SaveChangesAsync(ct);
        await cache.RemoveAsync(EvaluationMapping.CriteriaCacheKey(r.Id), ct);
        return ApiResponse<Guid>.Ok(r.Id, "Xóa kỳ đánh giá thành công");
    }
}

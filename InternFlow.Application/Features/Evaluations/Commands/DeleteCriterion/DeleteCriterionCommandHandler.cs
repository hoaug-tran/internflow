using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.DeleteCriterion;

public class DeleteCriterionCommandHandler(
    ICriterionRepository criteria,
    IEvaluationRepository evaluations,
    IUnitOfWork uow,
    ICacheService cache)
    : IRequestHandler<DeleteCriterionCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(DeleteCriterionCommand r, CancellationToken ct)
    {
        var c = await criteria.GetByIdAsync(r.Id, r.PeriodId, ct)
            ?? throw new NotFoundException("Không tìm thấy tiêu chí.");

        if (await evaluations.IsCriterionUsedAsync(r.Id, ct))
            throw new ConflictException("Tiêu chí đã được dùng trong phiếu đánh giá, không thể xóa.");

        criteria.Remove(c);
        await uow.SaveChangesAsync(ct);
        await cache.RemoveAsync(EvaluationMapping.CriteriaCacheKey(r.PeriodId), ct);
        return ApiResponse<Guid>.Ok(r.Id, "Xóa tiêu chí thành công");
    }
}

using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.UpdateCriterion;

public class UpdateCriterionCommandHandler(
    ICriterionRepository criteria,
    IUnitOfWork uow,
    ICacheService cache)
    : IRequestHandler<UpdateCriterionCommand, ApiResponse<CriterionDto>>
{
    public async Task<ApiResponse<CriterionDto>> Handle(UpdateCriterionCommand r, CancellationToken ct)
    {
        var c = await criteria.GetByIdAsync(r.Id, r.PeriodId, ct)
            ?? throw new NotFoundException("Không tìm thấy tiêu chí.");

        if (r.IsActive)
        {
            var othersTotal = await criteria.GetActiveTotalWeightAsync(r.PeriodId, r.Id, ct);
            if (othersTotal + r.Weight > EvaluationMapping.MaxTotalWeight)
                throw new BadRequestException($"Tổng trọng số vượt 100 (các tiêu chí khác {othersTotal}, sửa thành {r.Weight}).");
        }

        c.Name = r.Name.Trim();
        c.Description = r.Description;
        c.Weight = r.Weight;
        c.DisplayOrder = r.DisplayOrder;
        c.IsActive = r.IsActive;
        await uow.SaveChangesAsync(ct);
        await cache.RemoveAsync(EvaluationMapping.CriteriaCacheKey(r.PeriodId), ct);
        return ApiResponse<CriterionDto>.Ok(c.ToDto(), "Cập nhật tiêu chí thành công");
    }
}

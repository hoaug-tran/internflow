using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Evaluations.Commands.UpdateCriterion;

public class UpdateCriterionCommandHandler(IInternFlowDbContext context, ICacheService cache)
    : IRequestHandler<UpdateCriterionCommand, ApiResponse<CriterionDto>>
{
    public async Task<ApiResponse<CriterionDto>> Handle(UpdateCriterionCommand r, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(r.Name)) throw new BadRequestException("Tên tiêu chí không được để trống.");
        if (r.Weight <= 0 || r.Weight > EvaluationMapping.MaxTotalWeight)
            throw new BadRequestException("Trọng số phải trong khoảng (0, 100].");

        var c = await context.Criteria.FirstOrDefaultAsync(x => x.Id == r.Id && x.EvaluationPeriodId == r.PeriodId, ct)
            ?? throw new NotFoundException("Không tìm thấy tiêu chí.");

        if (r.IsActive)
        {
            var othersTotal = await context.Criteria
                .Where(x => x.EvaluationPeriodId == r.PeriodId && x.IsActive && x.Id != r.Id)
                .SumAsync(x => x.Weight, ct);
            if (othersTotal + r.Weight > EvaluationMapping.MaxTotalWeight)
                throw new BadRequestException($"Tổng trọng số vượt 100 (các tiêu chí khác {othersTotal}, sửa thành {r.Weight}).");
        }

        c.Name = r.Name.Trim();
        c.Description = r.Description;
        c.Weight = r.Weight;
        c.DisplayOrder = r.DisplayOrder;
        c.IsActive = r.IsActive;
        await context.SaveChangesAsync(ct);
        await cache.RemoveAsync(EvaluationMapping.CriteriaCacheKey(r.PeriodId), ct);
        return ApiResponse<CriterionDto>.Ok(c.ToDto(), "Cập nhật tiêu chí thành công");
    }
}

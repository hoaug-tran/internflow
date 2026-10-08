using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Domain.Evaluations;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Evaluations.Commands.CreateCriterion;

public class CreateCriterionCommandHandler(IInternFlowDbContext context, ICacheService cache)
    : IRequestHandler<CreateCriterionCommand, ApiResponse<CriterionDto>>
{
    public async Task<ApiResponse<CriterionDto>> Handle(CreateCriterionCommand r, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(r.Name)) throw new BadRequestException("Tên tiêu chí không được để trống.");
        if (r.Weight <= 0 || r.Weight > EvaluationMapping.MaxTotalWeight)
            throw new BadRequestException("Trọng số phải trong khoảng (0, 100].");

        if (!await context.EvaluationPeriods.AnyAsync(p => p.Id == r.PeriodId, ct))
            throw new NotFoundException("Không tìm thấy kỳ đánh giá.");

        var currentTotal = await context.Criteria
            .Where(c => c.EvaluationPeriodId == r.PeriodId && c.IsActive)
            .SumAsync(c => c.Weight, ct);
        if (currentTotal + r.Weight > EvaluationMapping.MaxTotalWeight)
            throw new BadRequestException($"Tổng trọng số vượt 100 (hiện tại {currentTotal}, thêm {r.Weight}).");

        var entity = new Criterion
        {
            EvaluationPeriodId = r.PeriodId,
            Name = r.Name.Trim(),
            Description = r.Description,
            Weight = r.Weight,
            DisplayOrder = r.DisplayOrder
        };
        context.Criteria.Add(entity);
        await context.SaveChangesAsync(ct);
        await cache.RemoveAsync(EvaluationMapping.CriteriaCacheKey(r.PeriodId), ct);
        return ApiResponse<CriterionDto>.Ok(entity.ToDto(), "Tạo tiêu chí thành công");
    }
}

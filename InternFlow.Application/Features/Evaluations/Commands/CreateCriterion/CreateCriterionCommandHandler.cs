using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Domain.Evaluations;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.CreateCriterion;

public class CreateCriterionCommandHandler(
    IEvaluationPeriodRepository periods,
    ICriterionRepository criteria,
    IUnitOfWork uow,
    ICacheService cache)
    : IRequestHandler<CreateCriterionCommand, ApiResponse<CriterionDto>>
{
    public async Task<ApiResponse<CriterionDto>> Handle(CreateCriterionCommand r, CancellationToken ct)
    {
        if (!await periods.ExistsAsync(r.PeriodId, ct))
            throw new NotFoundException("Không tìm thấy kỳ đánh giá.");

        var currentTotal = await criteria.GetActiveTotalWeightAsync(r.PeriodId, null, ct);
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
        criteria.Add(entity);
        await uow.SaveChangesAsync(ct);
        await cache.RemoveAsync(EvaluationMapping.CriteriaCacheKey(r.PeriodId), ct);
        return ApiResponse<CriterionDto>.Ok(entity.ToDto(), "Tạo tiêu chí thành công");
    }
}

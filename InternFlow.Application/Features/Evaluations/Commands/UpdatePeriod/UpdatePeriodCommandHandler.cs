using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;

namespace InternFlow.Application.Features.Evaluations.Commands.UpdatePeriod;

public class UpdatePeriodCommandHandler(IEvaluationPeriodRepository periods, IUnitOfWork uow)
    : IRequestHandler<UpdatePeriodCommand, ApiResponse<EvaluationPeriodDto>>
{
    public async Task<ApiResponse<EvaluationPeriodDto>> Handle(UpdatePeriodCommand r, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(r.Name)) throw new BadRequestException("Tên kỳ đánh giá không được để trống.");
        if (r.EndDate < r.StartDate) throw new BadRequestException("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");

        var p = await periods.GetByIdAsync(r.Id, ct)
            ?? throw new NotFoundException("Không tìm thấy kỳ đánh giá.");

        p.Name = r.Name.Trim();
        p.Description = r.Description;
        p.StartDate = r.StartDate;
        p.EndDate = r.EndDate;
        await uow.SaveChangesAsync(ct);
        return ApiResponse<EvaluationPeriodDto>.Ok(p.ToDto(), "Cập nhật kỳ đánh giá thành công");
    }
}

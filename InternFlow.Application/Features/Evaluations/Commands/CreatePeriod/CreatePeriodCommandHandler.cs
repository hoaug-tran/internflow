using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Domain.Evaluations;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Evaluations.Commands.CreatePeriod;

public class CreatePeriodCommandHandler(IInternFlowDbContext context)
    : IRequestHandler<CreatePeriodCommand, ApiResponse<EvaluationPeriodDto>>
{
    public async Task<ApiResponse<EvaluationPeriodDto>> Handle(CreatePeriodCommand r, CancellationToken ct)
    {
        if (!await context.Internships.AnyAsync(i => i.Id == r.InternshipId, ct))
            throw new NotFoundException("Không tìm thấy đợt thực tập.");

        var entity = new EvaluationPeriod
        {
            InternshipId = r.InternshipId,
            Name = r.Name.Trim(),
            Description = r.Description,
            StartDate = r.StartDate,
            EndDate = r.EndDate
        };
        context.EvaluationPeriods.Add(entity);
        await context.SaveChangesAsync(ct);
        return ApiResponse<EvaluationPeriodDto>.Ok(entity.ToDto(), "Tạo kỳ đánh giá thành công");
    }
}

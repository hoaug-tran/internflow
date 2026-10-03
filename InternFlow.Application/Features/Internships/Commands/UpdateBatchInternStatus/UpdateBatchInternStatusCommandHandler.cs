using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Internships;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Internships.Commands.UpdateBatchInternStatus;

public class UpdateBatchInternStatusCommandHandler : IRequestHandler<UpdateBatchInternStatusCommand, ApiResponse<bool>>
{
    private readonly IInternFlowDbContext _context;

    public UpdateBatchInternStatusCommandHandler(IInternFlowDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateBatchInternStatusCommand request, CancellationToken cancellationToken)
    {
        var batchIntern = await _context.BatchInterns
            .FirstOrDefaultAsync(bi => bi.InternshipId == request.InternshipId && bi.InternId == request.InternId, cancellationToken);

        if (batchIntern == null)
        {
            return ApiResponse<bool>.Fail("Không tìm thấy thực tập sinh trong đợt thực tập này.");
        }

        batchIntern.Status = request.NewStatus;
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Ok(true, $"Cập nhật trạng thái thực tập sinh sang '{request.NewStatus}' thành công.");
    }
}

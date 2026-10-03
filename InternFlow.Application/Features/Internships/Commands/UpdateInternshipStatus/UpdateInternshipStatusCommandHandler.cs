using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Internships;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Internships.Commands.UpdateInternshipStatus;

public class UpdateInternshipStatusCommandHandler : IRequestHandler<UpdateInternshipStatusCommand, ApiResponse<bool>>
{
    private readonly IInternFlowDbContext _context;

    public UpdateInternshipStatusCommandHandler(IInternFlowDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateInternshipStatusCommand request, CancellationToken cancellationToken)
    {
        var internship = await _context.Internships
            .FirstOrDefaultAsync(i => i.Id == request.InternshipId, cancellationToken);

        if (internship == null)
        {
            return ApiResponse<bool>.Fail("Không tìm thấy đợt thực tập yêu cầu.");
        }

        if (internship.Status == InternshipStatus.Closed)
        {
            return ApiResponse<bool>.Fail("Đợt thực tập đã Đóng (Closed) và không thể thay đổi trạng thái.");
        }

        if (internship.Status == request.NewStatus)
        {
            return ApiResponse<bool>.Ok(true, "Trạng thái đợt thực tập không thay đổi.");
        }

        // Quy tắc chuyển dịch: Planned -> Active, Closed. Active -> Closed.
        if (internship.Status == InternshipStatus.Active && request.NewStatus == InternshipStatus.Planned)
        {
            return ApiResponse<bool>.Fail("Không thể chuyển đợt thực tập đang hoạt động (Active) về trạng thái Kế hoạch (Planned).");
        }

        internship.Status = request.NewStatus;
        internship.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.Ok(true, $"Cập nhật trạng thái đợt thực tập sang '{request.NewStatus}' thành công.");
    }
}

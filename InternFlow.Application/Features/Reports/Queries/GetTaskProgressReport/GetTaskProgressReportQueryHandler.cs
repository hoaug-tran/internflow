using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Reports.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;

namespace InternFlow.Application.Features.Reports.Queries.GetTaskProgressReport;

public class GetTaskProgressReportQueryHandler(IReportQueryService reports, ICurrentUserService currentUser)
    : IRequestHandler<GetTaskProgressReportQuery, ApiResponse<List<TaskProgressRowDto>>>
{
    public async Task<ApiResponse<List<TaskProgressRowDto>>> Handle(
        GetTaskProgressReportQuery r, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenException("Không xác định được người dùng hiện tại.");

        var isAdmin = currentUser.IsInRole(nameof(UserRole.Admin));
        if (!isAdmin && !currentUser.IsInRole(nameof(UserRole.Mentor)))
            throw new ForbiddenException("Bạn không có quyền xem báo cáo này.");

        // Phạm vi dữ liệu (Mentor chỉ thấy intern mình phụ trách) do SP quyết định
        var rows = await reports.GetTaskProgressAsync(userId, isAdmin, r.ProjectId, ct);

        return ApiResponse<List<TaskProgressRowDto>>.Ok(rows.ToList());
    }
}

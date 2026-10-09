using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Reports.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Common;
using InternFlow.Shared.Exceptions;
using MediatR;

namespace InternFlow.Application.Features.Reports.Queries.GetMentorWorkloadReport;

public class GetMentorWorkloadReportQueryHandler(IReportQueryService reports, ICurrentUserService currentUser)
    : IRequestHandler<GetMentorWorkloadReportQuery, ApiResponse<List<MentorWorkloadRowDto>>>
{
    public async Task<ApiResponse<List<MentorWorkloadRowDto>>> Handle(
        GetMentorWorkloadReportQuery r, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new ForbiddenException("Không xác định được người dùng hiện tại.");

        var isAdmin = currentUser.IsInRole(nameof(UserRole.Admin));
        if (!isAdmin && !currentUser.IsInRole(nameof(UserRole.Mentor)))
            throw new ForbiddenException("Bạn không có quyền xem báo cáo này.");

        if (r.From.HasValue && r.To.HasValue && r.From.Value > r.To.Value)
            throw new BadRequestException("from không được lớn hơn to.");

        var rows = await reports.GetMentorWorkloadAsync(userId, isAdmin, r.ProjectId, r.From, r.To, ct);

        return ApiResponse<List<MentorWorkloadRowDto>>.Ok(rows.ToList());
    }
}

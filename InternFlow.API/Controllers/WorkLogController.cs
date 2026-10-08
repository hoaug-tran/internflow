using InternFlow.API.Controllers.Conventions;
using InternFlow.Application.Features.WorkLogs.Commands.CreateWorkLog;
using InternFlow.Application.Features.WorkLogs.Commands.UpdateWorkLog;
using InternFlow.Application.Features.WorkLogs.DTOs;
using InternFlow.Application.Features.WorkLogs.Queries.GetWorkLogs;
using InternFlow.Shared.Common;
using InternFlow.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternFlow.API.Controllers;

/// <summary>
/// Quản lý nhật ký làm việc của intern. Mỗi worklog gắn với một task được giao.
/// </summary>
[Route("api/worklogs")]
[Authorize]
public class WorkLogsController : ApiControllerBase
{
    /// <summary>Intern ghi nhật ký làm việc cho task của mình.</summary>
    /// <remarks>
    /// **Phân quyền:** chỉ **Intern**.
    ///
    /// **Ràng buộc:**
    /// - Task phải được giao cho chính intern (403 nếu không).
    /// - Task không được ở trạng thái `Todo` (409 — chưa bắt đầu) hoặc `Cancelled` (409).
    /// - `WorkDate` không được ở tương lai, phải nằm trong thời gian đợt thực tập.
    /// - Task `Done` → chỉ ghi bù trong vòng 3 ngày.
    /// - `Hours` &gt; 0 và ≤ 12/bản ghi; tổng giờ/ngày ≤ 12.
    /// - Không trùng `(TaskItemId, UserId, WorkDate)` với bản ghi active.
    /// - Project không được `OnHold` hoặc `Cancelled`.
    /// - Intern phải đang active trong đợt.
    ///
    /// **Cache:** không ảnh hưởng cache hiện tại (report workload chưa làm).
    /// Khi làm report → invalidate `report:workload:{internshipId}`.
    /// </remarks>
    /// <response code="200">Ghi nhật ký thành công.</response>
    /// <response code="400">WorkDate tương lai.</response>
    /// <response code="401">Chưa đăng nhập.</response>
    /// <response code="403">Không phải Intern / task không thuộc mình.</response>
    /// <response code="404">Task không tồn tại.</response>
    /// <response code="409">Vi phạm rule (task Todo/Cancelled, quá 12h, trùng ngày, ...).</response>
    [HttpPost]
    public async Task<ApiResponse<WorkLogResponse>> Create(
        [FromBody] CreateWorkLogRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(new CreateWorkLogCommand(request), ct);
        return ApiResponse<WorkLogResponse>.Ok(result, "Ghi nhật ký thành công");
    }

    /// <summary>Cập nhật nhật ký (chỉ chủ sở hữu, trong vòng 7 ngày).</summary>
    /// <remarks>
    /// **Phân quyền:** chỉ owner của worklog.
    ///
    /// **Ràng buộc:**
    /// - Chỉ sửa được trong vòng **7 ngày** kể từ ngày tạo.
    /// - Validate lại các rule như khi tạo (WorkDate, Hours, ...).
    ///
    /// **Cache:** không ảnh hưởng cache hiện tại.
    /// </remarks>
    /// <response code="200">Cập nhật thành công.</response>
    /// <response code="401">Chưa đăng nhập.</response>
    /// <response code="403">Không phải owner.</response>
    /// <response code="404">Worklog không tồn tại.</response>
    /// <response code="409">Quá 7 ngày / vi phạm rule.</response>
    [HttpPatch("{id:guid}")]
    public async Task<ApiResponse<WorkLogResponse>> Update(
        Guid id,
        [FromBody] UpdateWorkLogRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateWorkLogCommand(id, request), ct);
        return ApiResponse<WorkLogResponse>.Ok(result, "Cập nhật nhật ký thành công");
    }

    /// <summary>Lấy danh sách nhật ký theo quyền.</summary>
    /// <remarks>
    /// **Phân quyền theo scope:**
    /// - **Admin:** xem tất cả (nên filter theo project/internship).
    /// - **Mentor:** chỉ worklog của intern mình phụ trách.
    /// - **Intern:** chỉ worklog của mình.
    ///
    /// **Filter hỗ trợ:**
    /// - `TaskItemId`, `ProjectId`, `UserId`, `From`, `To`.
    /// - Phân trang bắt buộc.
    ///
    /// **Cache:** Không cache — filter tự do + per-user scope (mentor có list intern động).
    /// </remarks>
    /// <response code="200">Danh sách nhật ký phân trang.</response>
    /// <response code="401">Chưa đăng nhập.</response>
    [HttpGet]
    public async Task<ApiResponse<PagedResult<WorkLogListItemResponse>>> GetList(
        [FromQuery] WorkLogQueryParameters parameters,
        CancellationToken ct)
    {
        var result = await Sender.Send(new GetWorkLogsQuery(parameters), ct);
        return ApiResponse<PagedResult<WorkLogListItemResponse>>.Ok(result);
    }
}
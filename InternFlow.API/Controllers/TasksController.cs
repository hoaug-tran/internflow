using InternFlow.API.Controllers.Conventions;
using InternFlow.Application.Features.Tasks.Commands.ChangeTaskStatus;
using InternFlow.Application.Features.Tasks.Commands.CreateTask;
using InternFlow.Application.Features.Tasks.Commands.UpdateTask;
using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Application.Features.Tasks.Queries.GetTaskById;
using InternFlow.Application.Features.Tasks.Queries.GetTasks;
using InternFlow.Shared.Common;
using InternFlow.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternFlow.API.Controllers;

/// <summary>
/// Quản lý task trong dự án. Trạng thái tuân theo state machine
/// (<c>Todo → InProgress → InReview → Done</c>), chuyển sai luồng trả 409.
/// </summary>
[Route("api")]
[Authorize]
public class TasksController : ApiControllerBase
{
    /// <summary>Tạo task và giao cho intern trong dự án.</summary>
    /// <remarks>
    /// **Phân quyền:** Admin hoặc Lead của dự án.
    ///
    /// **Ràng buộc:**
    /// - Assignee phải là user có role `Intern`, đang active.
    /// - Assignee phải là member active của project.
    /// - Assignee phải thuộc đợt thực tập của project.
    ///
    /// **Cache invalidate:**
    /// - `task:summary:project:{projectId}` — task mới xuất hiện trong summary.
    /// </remarks>
    /// <response code="200">Tạo task thành công.</response>
    /// <response code="401">Chưa đăng nhập.</response>
    /// <response code="403">Không phải Admin/Lead.</response>
    /// <response code="404">Dự án hoặc user không tồn tại.</response>
    /// <response code="409">Assignee sai role / không phải member / khác đợt.</response>
    [HttpPost("projects/{id:guid}/tasks")]
    public async Task<ApiResponse<TaskResponse>> Create(
        Guid id,
        [FromBody] CreateTaskRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(
            new CreateTaskCommand(id, request), ct);

        return ApiResponse<TaskResponse>.Ok(result, "Tạo task thành công");
    }

    /// <summary>Chuyển trạng thái task theo state machine.</summary>
    /// <remarks>
    /// **Phân quyền theo state machine:**
    /// - **Admin:** mọi transition hợp lệ.
    /// - **Mentor:** mọi transition hợp lệ trong dự án mình là member.
    /// - **Intern:** chỉ task được giao, chỉ các transition cho phép
    ///   (không tự chuyển `Review → Done`).
    ///
    /// **Ràng buộc:**
    /// - Transition sai luồng → 409 (ví dụ: `Todo → Done`).
    /// - Transition đúng luồng nhưng sai role → 403.
    /// - Task không thuộc scope → 404 (ẩn sự tồn tại).
    /// - Chuyển sang `Done` → set `CompletedAt`.
    /// - Chuyển ra khỏi `Done` → clear `CompletedAt`.
    ///
    /// **Cache invalidate:**
    /// - `task:summary:project:{projectId}` — status thay đổi → summary thay đổi.
    /// </remarks>
    /// <response code="200">Chuyển thành công.</response>
    /// <response code="401">Chưa đăng nhập.</response>
    /// <response code="403">Sai role với transition.</response>
    /// <response code="404">Task không tồn tại hoặc ngoài scope.</response>
    /// <response code="409">Chuyển sai luồng trạng thái.</response>
    [HttpPatch("tasks/{id:guid}/status")]
    public async Task<ApiResponse<TaskResponse>> ChangeStatus(
        Guid id,
        [FromBody] ChangeTaskStatusRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(
            new ChangeTaskStatusCommand(id, request.NewStatus), ct);

        return ApiResponse<TaskResponse>.Ok(result, "Chuyển trạng thái thành công");
    }

    /// <summary>Lấy danh sách task có filter, search, paging, sort.</summary>
    /// <remarks>
    /// **Phân quyền theo scope:**
    /// - **Admin:** tất cả task.
    /// - **Mentor:** task trong dự án mình là member.
    /// - **Intern:** chỉ task được giao cho mình (`AssigneeId == userId`).
    ///
    /// **Filter hỗ trợ:**
    /// - `ProjectId`, `Status`, `AssigneeId`, `Keyword` (tìm trong Title).
    /// - Sort whitelist: `dueDate`, `priority`, `createdAt`, `status`.
    /// - PageSize tối đa **50**.
    ///
    /// **Cache:** Không cache — filter/paging tự do + per-user scope → vô số key.
    /// </remarks>
    /// <response code="200">Danh sách task phân trang.</response>
    /// <response code="401">Chưa đăng nhập.</response>
    [HttpGet("tasks")]
    public async Task<ApiResponse<PagedResult<TaskListItemResponse>>> GetList(
        [FromQuery] TaskQueryParameters parameters,
        CancellationToken ct)
    {
        var result = await Sender.Send(new GetTasksQuery(parameters), ct);

        return ApiResponse<PagedResult<TaskListItemResponse>>.Ok(result);
    }

    /// <summary>Lấy chi tiết một task theo Id.</summary>
    /// <remarks>
    /// **Phân quyền theo scope:**
    /// - **Admin:** mọi task.
    /// - **Mentor:** task trong dự án mình là member.
    /// - **Intern:** chỉ task được giao cho mình.
    ///
    /// Task ngoài scope → **404** (không lộ sự tồn tại).
    ///
    /// **Cache:** Không cache — detail đọc-ghi liên tục.
    /// </remarks>
    /// <response code="200">Chi tiết task.</response>
    /// <response code="401">Chưa đăng nhập.</response>
    /// <response code="404">Task không tồn tại hoặc ngoài scope.</response>
    [HttpGet("tasks/{id:guid}")]
    public async Task<ApiResponse<TaskResponse>> GetById(
        Guid id,
        CancellationToken ct)
    {
        var result = await Sender.Send(new GetTaskByIdQuery(id), ct);

        return ApiResponse<TaskResponse>.Ok(result);
    }

    /// <summary>Cập nhật thông tin task (title, description, assignee, priority, dueDate).</summary>
    /// <remarks>
    /// **Phân quyền:** Admin hoặc Mentor trong dự án.
    ///
    /// **Ràng buộc:**
    /// - Task phải thuộc scope (404 nếu không).
    /// - Nếu đổi assignee → validate assignee mới (phải là Intern active, member của project).
    /// - Không đổi được `Status` qua endpoint này — dùng `PATCH /tasks/{id}/status`.
    ///
    /// **Cache invalidate:**
    /// - `task:summary:project:{projectId}` — title/assignee/priority/dueDate đổi → summary đổi.
    /// </remarks>
    /// <response code="200">Cập nhật thành công.</response>
    /// <response code="401">Chưa đăng nhập.</response>
    /// <response code="403">Không phải Admin/Mentor.</response>
    /// <response code="404">Task hoặc assignee mới không tồn tại.</response>
    /// <response code="409">Assignee mới sai role / không phải member.</response>
    [HttpPatch("tasks/{id:guid}")]
    public async Task<ApiResponse<TaskResponse>> Update(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(
            new UpdateTaskCommand(id, request), ct);

        return ApiResponse<TaskResponse>.Ok(result, "Cập nhật task thành công");
    }
}
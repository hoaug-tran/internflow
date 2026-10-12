using InternFlow.API.Controllers.Conventions;
using InternFlow.Application.Features.WorkLogs.Commands.CreateWorkLog;
using InternFlow.Application.Features.WorkLogs.Commands.DeleteWorkLog;
using InternFlow.Application.Features.WorkLogs.Commands.RestoreWorkLog;
using InternFlow.Application.Features.WorkLogs.Commands.UpdateWorkLog;
using InternFlow.Application.Features.WorkLogs.DTOs;
using InternFlow.Application.Features.WorkLogs.Queries.GetWorkLogById;
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

    [HttpPost]
    public async Task<ApiResponse<WorkLogResponse>> Create(
        [FromBody] CreateWorkLogRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(new CreateWorkLogCommand(request), ct);
        return ApiResponse<WorkLogResponse>.Ok(result, "Ghi nhật ký thành công");
    }

    /// <summary>Cập nhật nhật ký (chỉ chủ sở hữu, trong vòng 7 ngày).</summary>
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

    [HttpGet]
    public async Task<ApiResponse<PagedResult<WorkLogListItemResponse>>> GetList(
        [FromQuery] WorkLogQueryParameters parameters,
        CancellationToken ct)
    {
        var result = await Sender.Send(new GetWorkLogsQuery(parameters), ct);
        return ApiResponse<PagedResult<WorkLogListItemResponse>>.Ok(result);
    }
    /// <summary>Xóa nhật ký .</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ApiResponse> Delete(
    Guid id,
    CancellationToken ct)
    {
        await Sender.Send(new DeleteWorkLogCommand(id), ct);
        return ApiResponse.Ok("Đã xóa nhật ký");
    }

    /// <summary>Lấy chi tiết worklog theo Id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ApiResponse<WorkLogResponse>> GetById(
    Guid id,
    CancellationToken ct)
    {
        var result = await Sender.Send(new GetWorkLogByIdQuery(id), ct);
        return ApiResponse<WorkLogResponse>.Ok(result);
    }
    /// <summary>Khôi phục worklog đã xóa mềm trong vòng 7 ngày.</summary>
    /// <remarks>
    /// **Phân quyền:** Intern — chỉ worklog của mình.
    /// </remarks>
    [HttpPost("{id:guid}/restore")]
    public async Task<ApiResponse<WorkLogResponse>> Restore(
        Guid id,
        CancellationToken ct)
    {
        var result = await Sender.Send(new RestoreWorkLogCommand(id), ct);
        return ApiResponse<WorkLogResponse>.Ok(result, "Khôi phục worklog thành công");
    }

}

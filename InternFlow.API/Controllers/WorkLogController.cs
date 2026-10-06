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

[Route("api/worklogs")]
[Authorize]
public class WorkLogsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ApiResponse<WorkLogResponse>> Create(
        [FromBody] CreateWorkLogRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(new CreateWorkLogCommand(request), ct);
        return ApiResponse<WorkLogResponse>.Ok(result, "Ghi nhật ký thành công");
    }


    [HttpPatch("{id:guid}")]
    public async Task<ApiResponse<WorkLogResponse>> Update(
        Guid id,
        [FromBody] UpdateWorkLogRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(new UpdateWorkLogCommand(id, request), ct);
        return ApiResponse<WorkLogResponse>.Ok(result, "Cập nhật nhật ký thành công");
    }
    [HttpGet]
    public async Task<ApiResponse<PagedResult<WorkLogListItemResponse>>> GetList(
        [FromQuery] WorkLogQueryParameters parameters,
        CancellationToken ct)
    {
        var result = await Sender.Send(new GetWorkLogsQuery(parameters), ct);
        return ApiResponse<PagedResult<WorkLogListItemResponse>>.Ok(result);
    }
}

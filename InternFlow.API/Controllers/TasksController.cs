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

[Route("api")]
[Authorize]
public class TasksController : ApiControllerBase
{
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

    [HttpGet("tasks")]
    public async Task<ApiResponse<PagedResult<TaskListItemResponse>>> GetList(
        [FromQuery] TaskQueryParameters parameters,
        CancellationToken ct)
    {
        var result = await Sender.Send(new GetTasksQuery(parameters), ct);

        return ApiResponse<PagedResult<TaskListItemResponse>>.Ok(result);
    }

    [HttpGet("tasks/{id:guid}")]
    public async Task<ApiResponse<TaskResponse>> GetById(
        Guid id,
        CancellationToken ct)
    {
        var result = await Sender.Send(new GetTaskByIdQuery(id), ct);

        return ApiResponse<TaskResponse>.Ok(result);
    }

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

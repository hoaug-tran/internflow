using InternFlow.Application.Features.Tasks.Commands.ChangeTaskStatus;
using InternFlow.Application.Features.Tasks.Commands.CreateTask;
using InternFlow.Application.Features.Tasks.Commands.UpdateTask;
using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Application.Features.Tasks.Queries.GetTasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternFlow.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator) => _mediator = mediator;


    [HttpPost("projects/{id:guid}/tasks")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        Guid id,
        [FromBody] CreateTaskRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateTaskCommand(id, request), ct);


        return Created($"/api/tasks/{result.Id}", result);
    }
    [HttpPatch("tasks/{id:guid}/status")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeStatus(
    Guid id,
    [FromBody] ChangeTaskStatusRequest request,
    CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ChangeTaskStatusCommand(id, request.NewStatus), ct);
        return Ok(result);
    }

    [HttpGet("tasks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetList(
    [FromQuery] TaskQueryParameters parameters,
    CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTasksQuery(parameters), ct);
        return Ok(result);
    }

    [HttpPatch("tasks/{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
    Guid id,
    [FromBody] UpdateTaskRequest request,
    CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateTaskCommand(id, request), ct);
        return Ok(result);
    }
}

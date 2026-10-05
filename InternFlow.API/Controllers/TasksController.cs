using InternFlow.Application.Features.Tasks.Commands.CreateTask;
using InternFlow.Application.Features.Tasks.DTOs;
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
}

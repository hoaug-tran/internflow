using InternFlow.Application.Features.Projects.Commands.AddProjectMember;
using InternFlow.Application.Features.Projects.Commands.CreateProject;
using InternFlow.Application.Features.Projects.Commands.RemoveProjectMember;
using InternFlow.Application.Features.Projects.Commands.UpdateProject;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Application.Features.Projects.Queries.GetProjectMembers;
using InternFlow.Application.Features.Projects.Queries.GetProjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternFlow.API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// POST /api/projects — Tạo project mới.
    /// Chỉ Admin/Mentor.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken ct)
    {
        var command = new CreateProjectCommand(request);
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// GET /api/projects — List project có phân trang.
    /// Scope theo role: Admin thấy tất cả, Mentor/Intern chỉ thấy dự án của mình.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetList(
        ProjectQueryParameters parameters,
        CancellationToken ct
    )
    {
        var result = await _mediator.Send(new GetProjectsQuery(parameters), ct);
        return Ok(result);
    }
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateProjectCommand(id, request), ct);
        return Ok(result);
    }
    public async Task<IActionResult> AddMember(
    Guid id,
    [FromBody] AddProjectMemberRequest request,
    CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AddProjectMemberCommand(id, request), ct);
        return CreatedAtAction(nameof(GetMembers), new { id }, result);
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveMember(
    Guid id,
    Guid userId,
    CancellationToken ct)
    {
        await _mediator.Send(new RemoveProjectMemberCommand(id, userId), ct);
        return NoContent();
    }
    [HttpGet("{id:guid}/members")]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMembers(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProjectMembersQuery(id), ct);
        return Ok(result);
    }



    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id) => throw new NotImplementedException();
}

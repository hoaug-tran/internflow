using InternFlow.API.Controllers.Conventions;
using InternFlow.Application.Features.Projects.Commands.AddProjectMember;
using InternFlow.Application.Features.Projects.Commands.CreateProject;
using InternFlow.Application.Features.Projects.Commands.RemoveProjectMember;
using InternFlow.Application.Features.Projects.Commands.UpdateProject;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Application.Features.Projects.Queries.GetProjectMembers;
using InternFlow.Application.Features.Projects.Queries.GetProjects;
using InternFlow.Shared.Common;
using InternFlow.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternFlow.API.Controllers;

[Route("api/projects")]
[Authorize]
public class ProjectsController : ApiControllerBase
{
    [HttpPost]
    public async Task<ApiResponse<ProjectResponse>> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(
            new CreateProjectCommand(request), ct);

        return ApiResponse<ProjectResponse>.Ok(result, "Tạo dự án thành công");
    }
    [HttpGet]
    public async Task<ApiResponse<PagedResult<ProjectListItemResponse>>> GetList(
        [FromQuery] ProjectQueryParameters parameters,
        CancellationToken ct)
    {
        var result = await Sender.Send(
            new GetProjectsQuery(parameters), ct);

        return ApiResponse<PagedResult<ProjectListItemResponse>>.Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ApiResponse<ProjectResponse>> Update(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(
            new UpdateProjectCommand(id, request), ct);

        return ApiResponse<ProjectResponse>.Ok(result, "Cập nhật dự án thành công");
    }

    [HttpPost("{id:guid}/members")]
    public async Task<ApiResponse<ProjectMemberResponse>> AddMember(
        Guid id,
        [FromBody] AddProjectMemberRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(
            new AddProjectMemberCommand(id, request), ct);

        return ApiResponse<ProjectMemberResponse>.Ok(result, "Thêm thành viên thành công");
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<ApiResponse> RemoveMember(
        Guid id,
        Guid userId,
        CancellationToken ct)
    {
        await Sender.Send(new RemoveProjectMemberCommand(id, userId), ct);

        return ApiResponse.Ok("Đã xóa thành viên khỏi dự án");
    }

    [HttpGet("{id:guid}/members")]
    public async Task<ApiResponse<IReadOnlyList<ProjectMemberResponse>>> GetMembers(
        Guid id,
        CancellationToken ct)
    {
        var result = await Sender.Send(new GetProjectMembersQuery(id), ct);

        return ApiResponse<IReadOnlyList<ProjectMemberResponse>>.Ok(result);
    }
}

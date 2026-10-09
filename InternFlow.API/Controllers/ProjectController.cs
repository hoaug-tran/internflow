using InternFlow.API.Controllers.Conventions;
using InternFlow.Application.Features.Projects.Commands.AddProjectMember;
using InternFlow.Application.Features.Projects.Commands.CreateProject;
using InternFlow.Application.Features.Projects.Commands.RemoveProjectMember;
using InternFlow.Application.Features.Projects.Commands.UpdateProject;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Application.Features.Projects.Queries.GetProjectMembers;
using InternFlow.Application.Features.Projects.Queries.GetProjects;
using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Application.Features.Tasks.Queries.GetTaskSummary;
using InternFlow.Shared.Common;
using InternFlow.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternFlow.API.Controllers;

/// <summary>
/// Quản lý dự án, thành viên và báo cáo tổng hợp task.
/// Phân quyền theo role + scope dữ liệu (Admin toàn quyền, Mentor/Intern giới hạn theo project).
/// </summary>
[Route("api/projects")]
[Authorize]
public class ProjectsController : ApiControllerBase
{
    /// <summary>Tạo dự án mới trong một đợt thực tập.</summary>
    /// <remarks>
    /// **Phân quyền:** Admin hoặc Mentor.
    /// </remarks>
    [HttpPost]
    public async Task<ApiResponse<ProjectResponse>> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken ct)
    {
        var result = await Sender.Send(
            new CreateProjectCommand(request), ct);

        return ApiResponse<ProjectResponse>.Ok(result, "Tạo dự án thành công");
    }

    /// <summary>Lấy danh sách dự án có phân trang, lọc theo quyền.</summary>
    /// <remarks>
    /// **Phân quyền theo scope:**
    /// - **Admin:** thấy tất cả dự án.
    /// - **Mentor:** chỉ thấy dự án mình tạo hoặc là thành viên.
    /// - **Intern:** chỉ thấy dự án mình là member.
    /// </remarks>
    [HttpGet]
    public async Task<ApiResponse<PagedResult<ProjectListItemResponse>>> GetList(
        [FromQuery] ProjectQueryParameters parameters,
        CancellationToken ct)
    {
        var result = await Sender.Send(
            new GetProjectsQuery(parameters), ct);

        return ApiResponse<PagedResult<ProjectListItemResponse>>.Ok(result);
    }

    /// <summary>Cập nhật thông tin dự án.</summary>
    /// <remarks>
    /// **Phân quyền:** Admin hoặc Lead của dự án.
    /// </remarks>
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

    /// <summary>Thêm thành viên vào dự án.</summary>
    /// <remarks>
    /// **Phân quyền:** Admin hoặc Lead của dự án.
    ///
    /// **Ràng buộc:**
    /// - User phải tồn tại và có role `Intern` hoặc `Mentor` (409 nếu sai role).
    /// - Nếu thêm **Intern**: phải thuộc đợt thực tập của dự án (409 nếu không).
    /// - Nếu **Mentor** thêm **Intern**: mentor phải đang phụ trách intern đó (403 nếu không).
    /// - Member chưa từng tồn tại, hoặc đã bị xóa mềm → reactivate (200).
    /// - Member đang active → 409 (đã là thành viên).
    ///
    /// **Cache invalidate:**
    /// - `project:members:{projectId}` — danh sách member thay đổi.
    /// - `task:summary:project:{projectId}` — summary có `ByAssignee` thay đổi.
    /// </remarks>
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

    /// <summary>Xóa thành viên khỏi dự án (soft delete).</summary>
    /// <remarks>
    /// **Phân quyền:** Admin hoặc Lead của dự án.
    /// </remarks>
    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<ApiResponse> RemoveMember(
        Guid id,
        Guid userId,
        CancellationToken ct)
    {
        await Sender.Send(new RemoveProjectMemberCommand(id, userId), ct);

        return ApiResponse.Ok("Đã xóa thành viên khỏi dự án");
    }

    /// <summary>Lấy danh sách thành viên active của dự án.</summary>
    /// <remarks>
    /// **Phân quyền:** Admin hoặc member của dự án (403 nếu không).
    /// </remarks>
    [HttpGet("{id:guid}/members")]
    public async Task<ApiResponse<IReadOnlyList<ProjectMemberResponse>>> GetMembers(
        Guid id,
        CancellationToken ct)
    {
        var result = await Sender.Send(new GetProjectMembersQuery(id), ct);

        return ApiResponse<IReadOnlyList<ProjectMemberResponse>>.Ok(result);
    }

    /// <summary>Lấy báo cáo tổng hợp task theo dự án.</summary>
    /// <remarks>
    /// **Phân quyền:** Admin hoặc member của dự án (403 nếu không)
    /// </remarks>
    [HttpGet("{id:guid}/tasks/summary")]
    public async Task<ApiResponse<TaskSummaryResponse>> GetTaskSummary(
        Guid id,
        CancellationToken ct)
    {
        var result = await Sender.Send(new GetTaskSummaryQuery(id), ct);

        return ApiResponse<TaskSummaryResponse>.Ok(result);
    }
}

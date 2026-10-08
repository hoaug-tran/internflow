using InternFlow.API.Controllers.Conventions;
using InternFlow.Application.Features.Internships.Commands.AddBatchIntern;
using InternFlow.Application.Features.Internships.Commands.AddBatchMentor;
using InternFlow.Application.Features.Internships.Commands.CreateInternship;
using InternFlow.Application.Features.Internships.Commands.UpdateBatchInternStatus;
using InternFlow.Application.Features.Internships.Commands.UpdateInternshipStatus;
using InternFlow.Application.Features.Internships.DTOs;
using InternFlow.Application.Features.Internships.Queries.GetInternshipById;
using InternFlow.Application.Features.Internships.Queries.GetInternships;
using InternFlow.Application.Features.MentorAssignments.Commands.AssignMentor;
using InternFlow.Application.Features.MentorAssignments.Commands.EndMentorAssignment;
using InternFlow.Application.Features.MentorAssignments.DTOs;
using InternFlow.Application.Features.MentorAssignments.Queries.GetMentorAssignments;
using InternFlow.Domain.Internships;
using InternFlow.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternFlow.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class InternshipsController : ApiControllerBase
{
    /// <summary>
    /// Tạo mới một kỳ thực tập (Chỉ Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ApiResponse<InternshipDto>> Create(
        [FromBody] CreateInternshipRequest request,
        CancellationToken ct)
    {
        return await Sender.Send(new CreateInternshipCommand(request.Name, request.StartDate, request.EndDate), ct);
    }

    /// <summary>
    /// Lấy danh sách kỳ thực tập (Hỗ trợ lọc theo Status và tìm kiếm theo Tên)
    /// </summary>
    [HttpGet]
    public async Task<ApiResponse<List<InternshipDto>>> GetAll(
        [FromQuery] InternshipStatus? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        return await Sender.Send(new GetInternshipsQuery(status, search, page, pageSize), ct);
    }

    /// <summary>
    /// Xem chi tiết một kỳ thực tập kèm danh sách Mentors và Interns
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ApiResponse<InternshipDetailDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        return await Sender.Send(new GetInternshipByIdQuery(id), ct);
    }

    /// <summary>
    /// Cập nhật trạng thái kỳ thực tập (Planned -> Active -> Closed)
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ApiResponse<bool>> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateInternshipStatusRequest request,
        CancellationToken ct)
    {
        return await Sender.Send(new UpdateInternshipStatusCommand(id, request.NewStatus), ct);
    }

    /// <summary>
    /// Thêm Mentor vào kỳ thực tập (Chỉ Admin)
    /// </summary>
    [HttpPost("{id:guid}/mentors")]
    [Authorize(Roles = "Admin")]
    public async Task<ApiResponse<bool>> AddMentor(
        [FromRoute] Guid id,
        [FromBody] AddBatchMentorRequest request,
        CancellationToken ct)
    {
        return await Sender.Send(new AddBatchMentorCommand(id, request.MentorId), ct);
    }

    /// <summary>
    /// Thêm Intern vào kỳ thực tập (Chỉ Admin)
    /// </summary>
    [HttpPost("{id:guid}/interns")]
    [Authorize(Roles = "Admin")]
    public async Task<ApiResponse<bool>> AddIntern(
        [FromRoute] Guid id,
        [FromBody] AddBatchInternRequest request,
        CancellationToken ct)
    {
        return await Sender.Send(new AddBatchInternCommand(id, request.InternId, Notes: request.Notes), ct);
    }

    /// <summary>
    /// Cập nhật trạng thái của Intern trong đợt (InProgress, Completed, DroppedOut, Failed)
    /// </summary>
    [HttpPut("{id:guid}/interns/{internId:guid}/status")]
    [Authorize(Roles = "Admin,Mentor")]
    public async Task<ApiResponse<bool>> UpdateInternStatus(
        [FromRoute] Guid id,
        [FromRoute] Guid internId,
        [FromBody] UpdateBatchInternStatusRequest request,
        CancellationToken ct)
    {
        return await Sender.Send(new UpdateBatchInternStatusCommand(id, internId, request.NewStatus), ct);
    }

    /// <summary>
    /// Phân công Mentor hướng dẫn Intern trong đợt (Chỉ Admin)
    /// </summary>
    [HttpPost("{id:guid}/assignments")]
    [Authorize(Roles = "Admin")]
    public async Task<ApiResponse<long>> AssignMentor(
        [FromRoute] Guid id,
        [FromBody] AssignMentorRequest request,
        CancellationToken ct)
    {
        return await Sender.Send(new AssignMentorCommand(id, request.InternId, request.MentorId), ct);
    }

    /// <summary>
    /// Kết thúc phân công hướng dẫn (Chỉ Admin)
    /// </summary>
    [HttpPut("{id:guid}/assignments/{assignmentId:long}/end")]
    [Authorize(Roles = "Admin")]
    public async Task<ApiResponse<bool>> EndAssignment(
        [FromRoute] Guid id,
        [FromRoute] long assignmentId,
        [FromBody] EndMentorAssignmentRequest? request,
        CancellationToken ct)
    {
        return await Sender.Send(new EndMentorAssignmentCommand(assignmentId, request?.EndReason), ct);
    }

    /// <summary>
    /// Lấy danh sách phân công Mentor trong kỳ thực tập
    /// </summary>
    [HttpGet("{id:guid}/assignments")]
    public async Task<ApiResponse<List<MentorAssignmentDto>>> GetAssignments(
        [FromRoute] Guid id,
        [FromQuery] Guid? mentorId,
        [FromQuery] Guid? internId,
        [FromQuery] bool onlyCurrent = false,
        CancellationToken ct = default)
    {
        return await Sender.Send(new GetMentorAssignmentsQuery(id, mentorId, internId, onlyCurrent), ct);
    }
}

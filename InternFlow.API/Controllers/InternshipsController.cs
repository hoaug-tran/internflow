using InternFlow.API.Extensions;
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
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InternshipsController : ControllerBase
{
    private readonly ISender _sender;

    public InternshipsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Tạo mới một kỳ thực tập (Chỉ Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<InternshipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InternshipDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateInternshipRequest request)
    {
        var currentUserId = User.GetUserId() ?? Guid.Empty;
        var command = new CreateInternshipCommand(request.Name, request.StartDate, request.EndDate, currentUserId);
        var result = await _sender.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách kỳ thực tập (Hỗ trợ lọc theo Status và tìm kiếm theo Tên)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<InternshipDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] InternshipStatus? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetInternshipsQuery(status, search, page, pageSize);
        var result = await _sender.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Xem chi tiết một kỳ thực tập kèm danh sách Mentors và Interns
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<InternshipDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<InternshipDetailDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var query = new GetInternshipByIdQuery(id);
        var result = await _sender.Send(query);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cập nhật trạng thái kỳ thực tập (Planned -> Active -> Closed)
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateInternshipStatusRequest request)
    {
        var command = new UpdateInternshipStatusCommand(id, request.NewStatus);
        var result = await _sender.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Thêm Mentor vào kỳ thực tập (Chỉ Admin)
    /// </summary>
    [HttpPost("{id:guid}/mentors")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddMentor(
        [FromRoute] Guid id,
        [FromBody] AddBatchMentorRequest request)
    {
        var currentUserId = User.GetUserId() ?? Guid.Empty;
        var command = new AddBatchMentorCommand(id, request.MentorId, currentUserId);
        var result = await _sender.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Thêm Intern vào kỳ thực tập (Chỉ Admin)
    /// </summary>
    [HttpPost("{id:guid}/interns")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddIntern(
        [FromRoute] Guid id,
        [FromBody] AddBatchInternRequest request)
    {
        var currentUserId = User.GetUserId() ?? Guid.Empty;
        var command = new AddBatchInternCommand(id, request.InternId, currentUserId, request.Notes);
        var result = await _sender.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Cập nhật trạng thái của Intern trong đợt (InProgress, Completed, DroppedOut, Failed)
    /// </summary>
    [HttpPut("{id:guid}/interns/{internId:guid}/status")]
    [Authorize(Roles = "Admin,Mentor")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateInternStatus(
        [FromRoute] Guid id,
        [FromRoute] Guid internId,
        [FromBody] UpdateBatchInternStatusRequest request)
    {
        var command = new UpdateBatchInternStatusCommand(id, internId, request.NewStatus);
        var result = await _sender.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Phân công Mentor hướng dẫn Intern trong đợt (Chỉ Admin)
    /// </summary>
    [HttpPost("{id:guid}/assignments")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<long>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<long>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignMentor(
        [FromRoute] Guid id,
        [FromBody] AssignMentorRequest request)
    {
        var currentUserId = User.GetUserId() ?? Guid.Empty;
        var command = new AssignMentorCommand(id, request.InternId, request.MentorId, currentUserId);
        var result = await _sender.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Kết thúc phân công hướng dẫn (Chỉ Admin)
    /// </summary>
    [HttpPut("{id:guid}/assignments/{assignmentId:long}/end")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EndAssignment(
        [FromRoute] Guid id,
        [FromRoute] long assignmentId,
        [FromBody] EndMentorAssignmentRequest? request)
    {
        var command = new EndMentorAssignmentCommand(assignmentId, request?.EndReason);
        var result = await _sender.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách phân công Mentor trong kỳ thực tập
    /// </summary>
    [HttpGet("{id:guid}/assignments")]
    [ProducesResponseType(typeof(ApiResponse<List<MentorAssignmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignments(
        [FromRoute] Guid id,
        [FromQuery] Guid? mentorId,
        [FromQuery] Guid? internId,
        [FromQuery] bool onlyCurrent = false)
    {
        var query = new GetMentorAssignmentsQuery(id, mentorId, internId, onlyCurrent);
        var result = await _sender.Send(query);
        return Ok(result);
    }
}

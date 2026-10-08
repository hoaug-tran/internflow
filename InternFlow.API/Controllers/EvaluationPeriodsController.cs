using InternFlow.API.Requests;
using InternFlow.API.Swagger;
using InternFlow.Application.Features.Evaluations.Commands.CreateCriterion;
using InternFlow.Application.Features.Evaluations.Commands.CreatePeriod;
using InternFlow.Application.Features.Evaluations.Commands.DeleteCriterion;
using InternFlow.Application.Features.Evaluations.Commands.DeletePeriod;
using InternFlow.Application.Features.Evaluations.Commands.UpdateCriterion;
using InternFlow.Application.Features.Evaluations.Commands.UpdatePeriod;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Application.Features.Evaluations.Queries.GetCriteria;
using InternFlow.Application.Features.Evaluations.Queries.GetPeriodById;
using InternFlow.Application.Features.Evaluations.Queries.GetPeriods;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternFlow.API.Controllers;

/// <summary>Quản lý kỳ đánh giá và tiêu chí: Admin toàn quyền, Mentor chỉ được GET.</summary>
[ApiController]
[Route("api/evaluation-periods")]
[Authorize]
[Produces("application/json")]
[ErrorResponses(403)]
public class EvaluationPeriodsController(IMediator mediator) : ControllerBase
{
    /// <summary>Lấy danh sách kỳ đánh giá, có thể lọc theo đợt thực tập.</summary>
    /// <remarks>
    /// - Mentor và Admin đều có thể xem.
    /// - Kết quả sắp xếp theo ngày bắt đầu giảm dần.
    /// </remarks>
    /// <param name="internshipId">Id đợt thực tập để lọc (tuỳ chọn).</param>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<EvaluationPeriodDto>>>> GetAll(
        [FromQuery] Guid? internshipId, CancellationToken ct)
        => await mediator.Send(new GetPeriodsQuery(internshipId), ct);

    /// <summary>Lấy chi tiết một kỳ đánh giá theo Id.</summary>
    /// <param name="id">Id kỳ đánh giá.</param>
    [HttpGet("{id:guid}")]
    [ErrorResponses(404)]
    public async Task<ActionResult<ApiResponse<EvaluationPeriodDto>>> GetById(Guid id, CancellationToken ct)
        => await mediator.Send(new GetPeriodByIdQuery(id), ct);

    /// <summary>Tạo kỳ đánh giá mới cho một đợt thực tập. Chỉ Admin.</summary>
    /// <remarks>
    /// - Đợt thực tập phải tồn tại (404 nếu không tìm thấy).
    /// - Tên kỳ không được để trống, ngày kết thúc phải &gt;= ngày bắt đầu (400).
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ErrorResponses(400, 404)]
    public async Task<ActionResult<ApiResponse<EvaluationPeriodDto>>> Create(
        [FromBody] CreatePeriodRequest body, CancellationToken ct)
        => await mediator.Send(new CreatePeriodCommand(body.InternshipId, body.Name, body.Description, body.StartDate, body.EndDate), ct);

    /// <summary>Cập nhật thông tin kỳ đánh giá. Chỉ Admin.</summary>
    /// <remarks>
    /// - Tên kỳ không được để trống, ngày kết thúc phải &gt;= ngày bắt đầu (400).
    /// </remarks>
    /// <param name="id">Id kỳ đánh giá cần cập nhật.</param>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ErrorResponses(400, 404)]
    public async Task<ActionResult<ApiResponse<EvaluationPeriodDto>>> Update(
        Guid id, [FromBody] UpdatePeriodRequest body, CancellationToken ct)
        => await mediator.Send(new UpdatePeriodCommand(id, body.Name, body.Description, body.StartDate, body.EndDate), ct);

    /// <summary>Xóa kỳ đánh giá và toàn bộ tiêu chí của nó. Chỉ Admin.</summary>
    /// <remarks>
    /// - Trả 409 nếu kỳ đã có phiếu đánh giá — không thể xóa để bảo toàn dữ liệu.
    /// </remarks>
    /// <param name="id">Id kỳ đánh giá cần xóa.</param>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ErrorResponses(404, 409)]
    public async Task<ActionResult<ApiResponse<Guid>>> Delete(Guid id, CancellationToken ct)
        => await mediator.Send(new DeletePeriodCommand(id), ct);

    /// <summary>Lấy danh sách tiêu chí của một kỳ đánh giá.</summary>
    /// <remarks>
    /// - Mentor và Admin đều có thể xem.
    /// - Kết quả sắp xếp theo DisplayOrder tăng dần, được cache để tăng hiệu năng.
    /// </remarks>
    /// <param name="periodId">Id kỳ đánh giá.</param>
    [HttpGet("{periodId:guid}/criteria")]
    [ErrorResponses(404)]
    public async Task<ActionResult<ApiResponse<List<CriterionDto>>>> GetCriteria(Guid periodId, CancellationToken ct)
        => await mediator.Send(new GetCriteriaQuery(periodId), ct);

    /// <summary>Tạo tiêu chí đánh giá mới cho một kỳ. Chỉ Admin.</summary>
    /// <remarks>
    /// - Tổng trọng số các tiêu chí active trong kỳ không được vượt 100 (400 nếu vi phạm).
    /// - Trọng số phải trong khoảng (0, 100] (400 nếu sai).
    /// </remarks>
    /// <param name="periodId">Id kỳ đánh giá mà tiêu chí thuộc về.</param>
    [HttpPost("{periodId:guid}/criteria")]
    [Authorize(Roles = "Admin")]
    [ErrorResponses(400, 404)]
    public async Task<ActionResult<ApiResponse<CriterionDto>>> CreateCriterion(
        Guid periodId, [FromBody] CreateCriterionRequest body, CancellationToken ct)
        => await mediator.Send(new CreateCriterionCommand(periodId, body.Name, body.Description, body.Weight, body.DisplayOrder), ct);

    /// <summary>Cập nhật tiêu chí đánh giá. Chỉ Admin.</summary>
    /// <remarks>
    /// - Tổng trọng số các tiêu chí active trong kỳ (ngoại trừ tiêu chí đang sửa) không được vượt 100 (400).
    /// - Trọng số phải trong khoảng (0, 100] (400 nếu sai).
    /// </remarks>
    /// <param name="periodId">Id kỳ đánh giá.</param>
    /// <param name="id">Id tiêu chí cần cập nhật.</param>
    [HttpPut("{periodId:guid}/criteria/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ErrorResponses(400, 404)]
    public async Task<ActionResult<ApiResponse<CriterionDto>>> UpdateCriterion(
        Guid periodId, Guid id, [FromBody] UpdateCriterionRequest body, CancellationToken ct)
        => await mediator.Send(new UpdateCriterionCommand(periodId, id, body.Name, body.Description, body.Weight, body.DisplayOrder, body.IsActive), ct);

    /// <summary>Xóa tiêu chí đánh giá. Chỉ Admin.</summary>
    /// <remarks>
    /// - Trả 409 nếu tiêu chí đã được dùng trong phiếu đánh giá — không thể xóa để bảo toàn dữ liệu.
    /// </remarks>
    /// <param name="periodId">Id kỳ đánh giá.</param>
    /// <param name="id">Id tiêu chí cần xóa.</param>
    [HttpDelete("{periodId:guid}/criteria/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ErrorResponses(404, 409)]
    public async Task<ActionResult<ApiResponse<Guid>>> DeleteCriterion(Guid periodId, Guid id, CancellationToken ct)
        => await mediator.Send(new DeleteCriterionCommand(periodId, id), ct);
}

using InternFlow.API.Swagger;
using InternFlow.Application.Features.Evaluations.Commands.CreateEvaluation;
using InternFlow.Application.Features.Evaluations.Commands.PublishEvaluation;
using InternFlow.Application.Features.Evaluations.Commands.UpdateEvaluation;
using InternFlow.Application.Features.Evaluations.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternFlow.Shared.Common;
using InternFlow.API.Requests;

namespace InternFlow.API.Controllers;

/// <summary>Phiếu đánh giá intern: Mentor chấm điểm, Admin có thể chấm thay.</summary>
[ApiController]
[Route("api/evaluations")]
[Authorize(Roles = "Mentor,Admin")]
[Produces("application/json")]
[ErrorResponses(403)] // áp dụng cho mọi action của controller
public class EvaluationsController(IMediator mediator) : ControllerBase
{
 

    /// <summary>Tạo phiếu đánh giá (trạng thái Draft) cho một intern trong một kỳ.</summary>
    /// <remarks>
    /// - Kỳ đánh giá phải đang **Open** (409 nếu không).
    /// - Mentor chỉ chấm intern mình phụ trách (403). Admin chấm thay được.
    /// - Mỗi intern chỉ có 1 phiếu/kỳ (409 nếu trùng).
    /// - Điểm mỗi tiêu chí từ 0 đến 10, tối đa 2 số thập phân (400 nếu sai).
    /// </remarks>
    [HttpPost]
    [ErrorResponses(400, 404, 409)]
    public async Task<ActionResult<ApiResponse<EvaluationDto>>> Create(
    [FromBody] CreateEvaluationRequest request, CancellationToken ct)
    => await mediator.Send(new CreateEvaluationCommand(
        request.EvaluationPeriodId, request.InternId, request.OverallComment, request.Scores), ct);

    /// <summary>Sửa điểm và nhận xét của phiếu đang Draft.</summary>
    /// <remarks>Phiếu đã **Published** thì không sửa được (409).</remarks>
    /// <param name="id">Id phiếu đánh giá.</param>
    [HttpPut("{id:guid}")]
    [ErrorResponses(400, 404, 409)]
    public async Task<ActionResult<ApiResponse<EvaluationDto>>> Update(
    Guid id, [FromBody] UpdateEvaluationRequest request, CancellationToken ct)
    => await mediator.Send(new UpdateEvaluationCommand(id, request.OverallComment, request.Scores), ct);

    /// <summary>Chốt phiếu đánh giá (Draft → Published).</summary>
    /// <remarks>Phải chấm đủ mọi tiêu chí đang dùng của kỳ (400 nếu thiếu). Published rồi thì không sửa được nữa.</remarks>
    /// <param name="id">Id phiếu đánh giá.</param>
    [HttpPost("{id:guid}/publish")]
    [ErrorResponses(400, 404, 409)]
    public async Task<ActionResult<ApiResponse<EvaluationDto>>> Publish(Guid id, CancellationToken ct)
        => await mediator.Send(new PublishEvaluationCommand(id), ct);
}

using InternFlow.API.Swagger;
using InternFlow.Application.Features.Evaluations.DTOs;
using InternFlow.Application.Features.Evaluations.Queries.GetMyEvaluationById;
using InternFlow.Application.Features.Evaluations.Queries.GetMyEvaluations;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternFlow.API.Controllers;

/// <summary>Intern xem kết quả đánh giá của chính mình.</summary>
[ApiController]
[Route("api/evaluations/me")]
[Authorize(Roles = "Intern")]
[Produces("application/json")]
[ErrorResponses(403)]
public class MyEvaluationsController(IMediator mediator) : ControllerBase
{
    /// <summary>Danh sách phiếu đã Published của tôi.</summary>
    /// <remarks>Chưa có phiếu nào thì trả danh sách rỗng (200).</remarks>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<MyEvaluationSummaryDto>>>> GetMine(CancellationToken ct)
        => await mediator.Send(new GetMyEvaluationsQuery(), ct);

    /// <summary>Chi tiết một phiếu của tôi (điểm từng tiêu chí, nhận xét).</summary>
    /// <remarks>404 nếu phiếu không tồn tại, còn Draft, hoặc của người khác.</remarks>
    /// <param name="id">Id phiếu đánh giá.</param>
    [HttpGet("{id:guid}")]
    [ErrorResponses(404)]
    public async Task<ActionResult<ApiResponse<EvaluationDto>>> GetMineById(Guid id, CancellationToken ct)
        => await mediator.Send(new GetMyEvaluationByIdQuery(id), ct);
}

using InternFlow.API.Swagger;
using InternFlow.Application.Features.Reports.DTOs;
using InternFlow.Application.Features.Reports.Queries.GetTaskProgressReport;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternFlow.API.Controllers;

/// <summary>Báo cáo: Mentor xem intern mình phụ trách, Admin xem tất cả.</summary>
[ApiController]
[Route("api/reports")]
[Authorize(Roles = "Mentor,Admin")]
[Produces("application/json")]
[ErrorResponses(403)]
public class ReportsController(IMediator mediator) : ControllerBase
{
    /// <summary>Báo cáo tiến độ task theo từng intern.</summary>
    /// <remarks>
    /// Mỗi dòng là một intern, gồm số task theo trạng thái, số task quá hạn, tỷ lệ hoàn thành và tổng giờ làm.
    /// Intern chưa có task vẫn xuất hiện với toàn số 0.
    /// </remarks>
    /// <param name="projectId">Chỉ tính trong một project (bỏ trống = tất cả).</param>
    [HttpGet("task-progress")]
    public async Task<ActionResult<ApiResponse<List<TaskProgressRowDto>>>> GetTaskProgress(
        [FromQuery] Guid? projectId, CancellationToken ct)
        => await mediator.Send(new GetTaskProgressReportQuery(projectId), ct);
}

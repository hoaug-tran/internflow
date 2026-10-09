using InternFlow.API.Swagger;
using InternFlow.Application.Features.Reports.DTOs;
using InternFlow.Application.Features.Reports.Queries.GetTaskProgressReport;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InternFlow.Application.Features.Reports.Queries.GetInternWorkloadReport;
using InternFlow.Application.Features.Reports.Queries.GetMentorWorkloadReport;
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

    /// <summary>Báo cáo khối lượng công việc theo từng intern.</summary>
    /// <remarks>
    /// Mỗi dòng là một intern: tổng giờ, số worklog, số ngày làm, số task, giờ trung bình mỗi ngày.
    /// Intern chưa có worklog vẫn xuất hiện với toàn số 0. Mentor chỉ thấy intern mình đang phụ trách.
    /// </remarks>
    /// <param name="fromDate">Từ ngày (theo ngày làm việc, bỏ trống = không giới hạn).</param>
    /// <param name="toDate">Đến ngày (bao gồm).</param>
    /// <param name="projectId">Chỉ tính trong một project (bỏ trống = tất cả).</param>
    [HttpGet("workload/interns")]
    [ErrorResponses(400)]
    public async Task<ActionResult<ApiResponse<List<InternWorkloadRowDto>>>> GetInternWorkload(
        [FromQuery(Name = "from")] DateOnly? fromDate,
        [FromQuery(Name = "to")] DateOnly? toDate,
        [FromQuery] Guid? projectId,
        CancellationToken ct)
        => await mediator.Send(new GetInternWorkloadReportQuery(fromDate, toDate, projectId), ct);


    /// <summary>Báo cáo khối lượng công việc theo từng mentor.</summary>
    /// <remarks>
    /// Mỗi dòng là một mentor: số intern đang phụ trách, tổng giờ của các intern đó, giờ trung bình mỗi intern.
    /// Mentor chỉ thấy dòng của chính mình, Admin thấy tất cả.
    /// </remarks>
    /// <param name="fromDate">Từ ngày (theo ngày làm việc, bỏ trống = không giới hạn).</param>
    /// <param name="toDate">Đến ngày (bao gồm).</param>
    /// <param name="projectId">Chỉ tính trong một project (bỏ trống = tất cả).</param>
    [HttpGet("workload/mentors")]
    [ErrorResponses(400)]
    public async Task<ActionResult<ApiResponse<List<MentorWorkloadRowDto>>>> GetMentorWorkload(
        [FromQuery(Name = "from")] DateOnly? fromDate,
        [FromQuery(Name = "to")] DateOnly? toDate,
        [FromQuery] Guid? projectId,
        CancellationToken ct)
        => await mediator.Send(new GetMentorWorkloadReportQuery(fromDate, toDate, projectId), ct);
}

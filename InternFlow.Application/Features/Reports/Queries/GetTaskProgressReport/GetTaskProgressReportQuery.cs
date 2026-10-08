using InternFlow.Application.Features.Reports.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Reports.Queries.GetTaskProgressReport;

public record GetTaskProgressReportQuery(Guid? ProjectId) : IRequest<ApiResponse<List<TaskProgressRowDto>>>;

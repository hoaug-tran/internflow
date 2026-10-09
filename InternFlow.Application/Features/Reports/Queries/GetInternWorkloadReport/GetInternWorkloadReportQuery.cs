using InternFlow.Application.Features.Reports.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Reports.Queries.GetInternWorkloadReport;

public record GetInternWorkloadReportQuery(DateOnly? From, DateOnly? To, Guid? ProjectId)
    : IRequest<ApiResponse<List<InternWorkloadRowDto>>>;

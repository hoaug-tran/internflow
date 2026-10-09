using InternFlow.Application.Features.Reports.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Reports.Queries.GetMentorWorkloadReport;

public record GetMentorWorkloadReportQuery(DateOnly? From, DateOnly? To, Guid? ProjectId)
    : IRequest<ApiResponse<List<MentorWorkloadRowDto>>>;

using InternFlow.Application.Features.WorkLogs.DTOs;
using InternFlow.Shared.Models;
using MediatR;

namespace InternFlow.Application.Features.WorkLogs.Queries.GetWorkLogs;

public sealed record GetWorkLogsQuery(WorkLogQueryParameters Parameters)
    : IRequest<PagedResult<WorkLogListItemResponse>>;

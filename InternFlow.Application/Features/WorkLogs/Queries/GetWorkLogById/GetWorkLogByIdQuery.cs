using InternFlow.Application.Features.WorkLogs.DTOs;
using MediatR;

namespace InternFlow.Application.Features.WorkLogs.Queries.GetWorkLogById;

public sealed record GetWorkLogByIdQuery(Guid Id)
    : IRequest<WorkLogResponse>;

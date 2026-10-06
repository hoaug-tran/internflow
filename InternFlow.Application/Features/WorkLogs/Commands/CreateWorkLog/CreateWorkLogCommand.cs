using InternFlow.Application.Features.WorkLogs.DTOs;
using MediatR;

namespace InternFlow.Application.Features.WorkLogs.Commands.CreateWorkLog;

public sealed record CreateWorkLogCommand(CreateWorkLogRequest Request)
    : IRequest<WorkLogResponse>;

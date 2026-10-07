using InternFlow.Application.Features.WorkLogs.DTOs;
using MediatR;

namespace InternFlow.Application.Features.WorkLogs.Commands.UpdateWorkLog;

public sealed record UpdateWorkLogCommand(
    Guid WorkLogId,
    UpdateWorkLogRequest Request)
    : IRequest<WorkLogResponse>;

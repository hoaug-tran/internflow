using InternFlow.Application.Features.WorkLogs.DTOs;
using MediatR;

namespace InternFlow.Application.Features.WorkLogs.Commands.RestoreWorkLog;

public sealed record RestoreWorkLogCommand(Guid Id)
    : IRequest<WorkLogResponse>;

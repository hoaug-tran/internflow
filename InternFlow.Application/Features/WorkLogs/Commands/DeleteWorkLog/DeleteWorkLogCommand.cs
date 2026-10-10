using MediatR;

namespace InternFlow.Application.Features.WorkLogs.Commands.DeleteWorkLog;

public sealed record DeleteWorkLogCommand(Guid Id) : IRequest<Unit>;

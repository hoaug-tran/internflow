using InternFlow.Application.Features.Tasks.DTOs;
using MediatR;

namespace InternFlow.Application.Features.Tasks.Commands.UpdateTask;

public sealed record UpdateTaskCommand(
    Guid TaskId,
    UpdateTaskRequest Request)
    : IRequest<TaskResponse>;

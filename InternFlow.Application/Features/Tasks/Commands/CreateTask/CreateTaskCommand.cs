using InternFlow.Application.Features.Tasks.DTOs;
using MediatR;

namespace InternFlow.Application.Features.Tasks.Commands.CreateTask;

public sealed record CreateTaskCommand(
    Guid ProjectId,
    CreateTaskRequest Request)
    : IRequest<TaskResponse>;

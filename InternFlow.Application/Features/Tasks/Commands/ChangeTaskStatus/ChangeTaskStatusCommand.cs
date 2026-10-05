using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Domain.Projects;
using TaskStatus = InternFlow.Domain.Projects.TaskStatus;
using MediatR;

namespace InternFlow.Application.Features.Tasks.Commands.ChangeTaskStatus;

public sealed record ChangeTaskStatusCommand(
    Guid TaskId,
    TaskStatus NewStatus)
    : IRequest<TaskResponse>;

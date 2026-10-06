using InternFlow.Application.Features.Tasks.DTOs;
using MediatR;

namespace InternFlow.Application.Features.Tasks.Queries.GetTaskById;

public sealed record GetTaskByIdQuery(Guid TaskId)
    : IRequest<TaskResponse>;

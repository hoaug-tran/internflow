using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Shared.Models;
using MediatR;

namespace InternFlow.Application.Features.Tasks.Queries.GetTasks;

public sealed record GetTasksQuery(TaskQueryParameters Parameters)
    : IRequest<PagedResult<TaskListItemResponse>>;

using InternFlow.Application.Common.Caching;
using InternFlow.Application.Features.Tasks.DTOs;
using MediatR;

namespace InternFlow.Application.Features.Tasks.Queries.GetTaskSummary;

[Cacheable("task:summary:project", ttlMinutes: 5)]
public sealed record GetTaskSummaryQuery(Guid ProjectId)
    : IRequest<TaskSummaryResponse>;
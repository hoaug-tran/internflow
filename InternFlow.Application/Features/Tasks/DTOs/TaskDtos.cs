using InternFlow.Domain.Projects;
using InternFlow.Shared.Models;

namespace InternFlow.Application.Features.Tasks.DTOs;

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    TaskPriority Priority,
    DateOnly? DueDate,
    Guid AssigneeId);

public sealed class TaskResponse
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public Guid? AssigneeId { get; init; }
    public string? AssigneeName { get; init; }
    public DateOnly? DueDate { get; init; }
    public DateTime? CompletedAt { get; init; }
    public Guid CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}


public sealed class TaskQueryParameters : PaginationQuery
{
    public Guid? ProjectId { get; set; }
    public string? Status { get; set; }
    public Guid? AssigneeId { get; set; }
}

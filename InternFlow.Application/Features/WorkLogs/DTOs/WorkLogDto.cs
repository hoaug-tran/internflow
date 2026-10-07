using InternFlow.Shared.Models;

namespace InternFlow.Application.Features.WorkLogs.DTOs;

public sealed record CreateWorkLogRequest(
    Guid TaskItemId,
    DateOnly WorkDate,
    decimal Hours,
    string Description);

public sealed record UpdateWorkLogRequest(
    DateOnly WorkDate,
    decimal Hours,
    string Description);

public sealed class WorkLogResponse
{
    public Guid Id { get; init; }
    public Guid TaskItemId { get; init; }
    public string TaskTitle { get; init; } = string.Empty;
    public Guid ProjectId { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public DateOnly WorkDate { get; init; }
    public decimal Hours { get; init; }
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed record WorkLogListItemResponse(
    Guid Id,
    Guid TaskItemId,
    string TaskTitle,
    Guid UserId,
    string UserName,
    DateOnly WorkDate,
    decimal Hours,
    string Description,
    DateTime CreatedAt);

public sealed class WorkLogQueryParameters : PaginationQuery
{
    public Guid? TaskItemId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? UserId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
}

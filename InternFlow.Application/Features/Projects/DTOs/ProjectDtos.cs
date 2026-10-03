using InternFlow.Shared.Models;

namespace InternFlow.Application.Features.Projects.DTOs;

public sealed record CreateProjectRequest(
    Guid InternshipId,
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate);

public sealed record ProjectResponse(
    Guid Id,
    Guid InternshipId,
    string Name,
    string? Description,
    string Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    Guid CreatedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record ProjectListItemResponse(
    Guid Id,
    Guid InternshipId,
    string Name,
    string Status,
    int MemberCount,
    int TaskCount,
    DateTime CreatedAt);

public sealed class ProjectQueryParameters : PaginationQuery
{
    public Guid? InternshipId { get; set; }
    public string? Status { get; set; }
}

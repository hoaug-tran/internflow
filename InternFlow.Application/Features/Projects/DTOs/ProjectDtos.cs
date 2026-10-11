using InternFlow.Domain.Projects;
using InternFlow.Shared.Models;

namespace InternFlow.Application.Features.Projects.DTOs;

public sealed record CreateProjectRequest(
    Guid InternshipId,
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate);
public sealed record UpdateProjectRequest(
    string? Name,
    string? Description,
    ProjectStatus Status,
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
public sealed record ProjectListItemResponse
(
        Guid id,
        Guid internshipId,
        string name,
        string status,
        int memberCount,
        int taskCount,
        DateTime createdAt);



public sealed class ProjectQueryParameters : PaginationQuery
{
    public Guid? InternshipId { get; set; }
    public string? Status { get; set; }
}
public sealed record AddProjectMemberRequest(
    Guid UserId,
    MemberRole Role);
public sealed record ProjectMemberResponse(
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    DateTime JoinedAt);

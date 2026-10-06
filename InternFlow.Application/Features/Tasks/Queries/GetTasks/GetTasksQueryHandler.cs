using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Extensions;
using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Constants;
using InternFlow.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Tasks.Queries.GetTasks;

public sealed class GetTasksQueryHandler
    : IRequestHandler<GetTasksQuery, PagedResult<TaskListItemResponse>>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetTasksQueryHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<TaskListItemResponse>> Handle(
        GetTasksQuery req, CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();
        var role = _currentUser.Role;
        var p = req.Parameters;

        var query = _db.Tasks.AsNoTracking();
        query = ApplyScope(query, userId, role);

        //Filter
        if (p.ProjectId.HasValue)
            query = query.Where(t => t.ProjectId == p.ProjectId.Value);

        if (p.Status.HasValue)
            query = query.Where(t => t.Status == p.Status.Value);

        if (p.AssigneeId.HasValue)
            query = query.Where(t => t.AssigneeId == p.AssigneeId.Value);

        if (!string.IsNullOrWhiteSpace(p.Keyword))
        {
            var kw = p.Keyword.Trim();
            query = query.Where(t => EF.Functions.Like(t.Title, $"%{kw}%"));
        }

        query = ApplySort(query, p.SortBy, p.SortDir);

        var dtoQuery = query.Select(t => new TaskListItemResponse(
            t.Id,
            t.ProjectId,
            t.Title,
            t.Status.ToString(),
            t.Priority.ToString(),
            t.AssigneeId,
            t.Assignee != null ? t.Assignee.FullName : null,
            t.DueDate,
            t.CreatedAt));

        return await dtoQuery.ToPagedResultAsync(p, ct);
    }

    private static IQueryable<TaskItem> ApplyScope(
        IQueryable<TaskItem> query, Guid userId, string? role)
    {
        return role switch
        {
            nameof(UserRole.Admin) => query,

            nameof(UserRole.Mentor) => query.Where(t =>
                t.Project.CreatedBy == userId
                || t.Project.Members.Any(m => m.UserId == userId && m.RemovedAt == null)),

            nameof(UserRole.Intern) => query.Where(t => t.AssigneeId == userId),

            _ => query.Where(_ => false)
        };
    }

    private static IQueryable<TaskItem> ApplySort(
        IQueryable<TaskItem> q, string? sortBy, string? sortDir)
    {
        var desc = SortDirections.IsDescending(sortDir);

        return (sortBy?.ToLowerInvariant()) switch
        {
            TaskSortFields.DueDate => desc
                ? q.OrderByDescending(t => t.DueDate).ThenBy(t => t.Id)
                : q.OrderBy(t => t.DueDate).ThenBy(t => t.Id),

            TaskSortFields.Priority => desc
                ? q.OrderByDescending(t => t.Priority).ThenBy(t => t.Id)
                : q.OrderBy(t => t.Priority).ThenBy(t => t.Id),

            TaskSortFields.Status => desc
                ? q.OrderByDescending(t => t.Status).ThenBy(t => t.Id)
                : q.OrderBy(t => t.Status).ThenBy(t => t.Id),

            TaskSortFields.CreatedAt or null or "" => desc
                ? q.OrderByDescending(t => t.CreatedAt).ThenBy(t => t.Id)
                : q.OrderBy(t => t.CreatedAt).ThenBy(t => t.Id),

            _ => q.OrderBy(t => t.CreatedAt).ThenBy(t => t.Id)
        };
    }
}

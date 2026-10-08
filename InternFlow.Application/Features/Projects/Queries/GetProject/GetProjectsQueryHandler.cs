using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Extensions;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Constants;
using InternFlow.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Projects.Queries.GetProjects;

public sealed class GetProjectsQueryHandler
    : IRequestHandler<GetProjectsQuery, PagedResult<ProjectListItemResponse>>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetProjectsQueryHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ProjectListItemResponse>> Handle(
        GetProjectsQuery req, CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();
        var p = req.Parameters;

        var query = _db.Projects
            .AsNoTracking()
            .ApplyProjectVisibilityScope(userId, _currentUser.Role);
        // Filter
        if (p.InternshipId.HasValue)
            query = query.Where(x => x.InternshipId == p.InternshipId.Value);

        if (!string.IsNullOrWhiteSpace(p.Keyword))
        {
            var kw = p.Keyword.Trim();
            query = query.Where(x => EF.Functions.Like(x.Name, $"%{kw}%"));
        }

        if (!string.IsNullOrWhiteSpace(p.Status))
            query = query.Where(x => x.Status.ToString() == p.Status);

        // Sort
        query = ApplySort(query, p.SortBy, p.SortDir);

        // Projection — EF dịch thành SQL, không load entity
        var dtoQuery = query.Select(x => new ProjectListItemResponse(
            x.Id,
            x.InternshipId,
            x.Name,
            x.Status.ToString(),
            x.Members.Count(m => m.RemovedAt == null),
            x.Tasks.Count,
            x.CreatedAt));

        return await dtoQuery.ToPagedResultAsync(p, ct);
    }

    private static IQueryable<Project> ApplySort(
        IQueryable<Project> q, string? sortBy, string? sortDir)
    {
        var desc = SortDirections.IsDescending(sortDir);

        return (sortBy?.ToLowerInvariant()) switch
        {
            ProjectSortFields.Name => desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name),
            ProjectSortFields.Status => desc ? q.OrderByDescending(x => x.Status) : q.OrderBy(x => x.Status),
            _ => desc ? q.OrderByDescending(x => x.CreatedAt) : q.OrderBy(x => x.CreatedAt)
        };
    }
}

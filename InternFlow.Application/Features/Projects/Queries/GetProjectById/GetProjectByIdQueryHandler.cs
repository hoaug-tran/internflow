using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Projects.Queries.GetProjectById;

public sealed class GetProjectByIdQueryHandler
    : IRequestHandler<GetProjectByIdQuery, ProjectResponse>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetProjectByIdQueryHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ProjectResponse> Handle(
        GetProjectByIdQuery req, CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();
        var isAdmin = _currentUser.IsInRole(UserRole.Admin.ToString());

        var project = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == req.ProjectId)
            .Select(p => new ProjectResponse(
    p.Id,
    p.InternshipId,
    p.Name,
    p.Description,
    p.Status.ToString(),
    p.StartDate,
    p.EndDate,
    p.CreatedBy,
    p.CreatedAt,
    p.UpdatedAt))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(
                $"Dự án {req.ProjectId} không tồn tại.");

        // Check Admin hoặc member
        if (!isAdmin)
        {
            var isMember = await _db.ProjectMembers
                .AsNoTracking()
                .AnyAsync(m => m.ProjectId == req.ProjectId
                            && m.UserId == userId
                            && m.RemovedAt == null, ct);

            if (!isMember)
                throw new NotFoundException(
                    $"Dự án {req.ProjectId} không tồn tại.");
        }

        return project;
    }
}

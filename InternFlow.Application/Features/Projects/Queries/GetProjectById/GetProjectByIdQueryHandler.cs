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
        await _db.EnsureCanViewProjectAsync(
            req.ProjectId, userId, _currentUser.Role, ct);

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
        return project;
    }
}

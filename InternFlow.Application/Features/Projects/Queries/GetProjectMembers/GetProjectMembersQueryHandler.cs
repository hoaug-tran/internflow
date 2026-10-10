using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Projects.Queries.GetProjectMembers;

public sealed class GetProjectMembersQueryHandler
    : IRequestHandler<GetProjectMembersQuery, IReadOnlyList<ProjectMemberResponse>>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetProjectMembersQueryHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ProjectMemberResponse>> Handle(
        GetProjectMembersQuery req, CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();

        await _db.EnsureCanViewProjectAsync(
            req.ProjectId, userId, _currentUser.Role, ct);

        var members = await _db.ProjectMembers
            .AsNoTracking()
            .Where(m => m.ProjectId == req.ProjectId
                     && m.RemovedAt == null)
            .OrderBy(m => m.JoinedAt)
            .Select(m => new ProjectMemberResponse(
                m.UserId,
                m.User.FullName,
                m.User.Email,
                m.Role.ToString(),
                m.JoinedAt))
            .ToListAsync(ct);

        return members;
    }
}

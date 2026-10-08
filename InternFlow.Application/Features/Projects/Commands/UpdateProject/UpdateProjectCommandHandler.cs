using AutoMapper;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Projects.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Projects.Commands.UpdateProject;

public sealed class UpdateProjectCommandHandler
    : IRequestHandler<UpdateProjectCommand, ProjectResponse>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public UpdateProjectCommandHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<ProjectResponse> Handle(
        UpdateProjectCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();

        await _db.EnsureCanManageProjectAsync(
            cmd.ProjectId, userId, _currentUser.Role, ct);

        var project = await _db.Projects
            .FirstAsync(p => p.Id == cmd.ProjectId, ct);

        _mapper.Map(cmd.Request, project);
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return _mapper.Map<ProjectResponse>(project);
    }
}

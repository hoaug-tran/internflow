using AutoMapper;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Projects.Commands.UpdateProject;

public sealed class UpdateProjectCommandHandler
    : IRequestHandler<UpdateProjectCommand, ProjectResponse>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;

    public UpdateProjectCommandHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser,
        IMapper mapper,
        IUnitOfWork uow)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
        _uow = uow;
    }

    public async Task<ProjectResponse> Handle(
        UpdateProjectCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();

        await _db.EnsureCanManageProjectAsync(
            cmd.ProjectId, userId, _currentUser.Role, ct);

        var project = await _db.Projects
            .FirstAsync(p => p.Id == cmd.ProjectId, ct);
        // Check trùng tên (chỉ khi đổi tên)
        if (!string.Equals(project.Name, cmd.Request.Name,
                StringComparison.OrdinalIgnoreCase))
        {
            var nameExists = await _db.Projects
                .AsNoTracking()
                .AnyAsync(p => p.InternshipId == project.InternshipId
                            && p.Name == cmd.Request.Name
                            && p.Id != project.Id, ct);

            if (nameExists)
                throw new ConflictException(
                    $"Dự án '{cmd.Request.Name}' đã tồn tại trong đợt này.");
        }

        _mapper.Map(cmd.Request, project);
        project.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ProjectResponse>(project);
    }
}

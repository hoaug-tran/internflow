using AutoMapper;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
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
        var userId = _currentUser.EnsureRole(UserRole.Admin, UserRole.Mentor);
        var isAdmin = _currentUser.IsInRole(UserRole.Admin.ToString());

        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == cmd.ProjectId, ct)
            ?? throw new NotFoundException(
                $"Dự án {cmd.ProjectId} không tồn tại.");

        if (!isAdmin)
        {
            var isLead = await _db.ProjectMembers
                .AsNoTracking()
                .AnyAsync(m => m.ProjectId == cmd.ProjectId
                            && m.UserId == userId
                            && m.Role == MemberRole.Lead
                            && m.RemovedAt == null, ct);

            if (!isLead)
                throw new ForbiddenException(
                "Chỉ Admin hoặc Lead của dự án mới được sửa.");
        }

        _mapper.Map(cmd.Request, project);
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return _mapper.Map<ProjectResponse>(project);
    }
}

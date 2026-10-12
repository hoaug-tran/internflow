using AutoMapper;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandHandler
    : IRequestHandler<CreateProjectCommand, ProjectResponse>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;

    public CreateProjectCommandHandler(
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
        CreateProjectCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.EnsureRole(UserRole.Admin, UserRole.Mentor);

        var internship = await _db.Internships
            .AsNoTracking()
            .AnyAsync(i => i.Id == cmd.Request.InternshipId && i.Status == Domain.Internships.InternshipStatus.Active, ct);

        if (!internship)
            throw new NotFoundException(
                $"Đợt {cmd.Request.InternshipId} không tồn tại hoặc không Active.");

        var exists = await _db.Projects.AnyAsync(p =>
            p.InternshipId == cmd.Request.InternshipId &&
            p.Name == cmd.Request.Name, ct);

        if (exists)
            throw new ConflictException($"Dự án '{cmd.Request.Name}' đã tồn tại.");

        var project = _mapper.Map<Project>(cmd.Request);
        var now = DateTime.UtcNow;

        project.Id = Guid.NewGuid();
        project.Status = ProjectStatus.Planned;
        project.CreatedBy = userId;
        project.CreatedAt = now;
        project.UpdatedAt = now;

        _db.Projects.Add(project);

        // Người tạo -> Lead
        _db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = userId,
            Role = MemberRole.Lead,
            JoinedAt = now
        });

        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ProjectResponse>(project);
    }
}

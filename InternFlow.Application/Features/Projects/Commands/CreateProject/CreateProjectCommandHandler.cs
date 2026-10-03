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

    public CreateProjectCommandHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<ProjectResponse> Handle(
        CreateProjectCommand cmd, CancellationToken ct)
    {
        // 1. Role check
        var userId = _currentUser.EnsureRole(UserRole.Admin, UserRole.Mentor);

        // 2. Internship tồn tại + Active
        var internship = await _db.Internships
            .AsNoTracking()
            .Where(i => i.Id == cmd.Request.InternshipId)
            .Select(i => new { i.Id, i.Status })
            .FirstOrDefaultAsync(ct);

        if (internship is null || internship.Status.ToString() != "Active")
            throw new NotFoundException(
                $"Đợt {cmd.Request.InternshipId} không tồn tại hoặc không Active.");

        // 3. Chặn trùng tên
        var exists = await _db.Projects.AnyAsync(p =>
            p.InternshipId == cmd.Request.InternshipId &&
            p.Name == cmd.Request.Name, ct);

        if (exists)
            throw new ConflictException($"Dự án '{cmd.Request.Name}' đã tồn tại.");

        // 4. Tạo entity — mapping thủ công
        var now = DateTime.UtcNow;
        var project = new Project
        {
            Id = Guid.NewGuid(),
            InternshipId = cmd.Request.InternshipId,
            Name = cmd.Request.Name,
            Description = cmd.Request.Description,
            Status = ProjectStatus.Planned,
            StartDate = cmd.Request.StartDate,
            EndDate = cmd.Request.EndDate,
            CreatedBy = userId,
            CreatedAt = now,
            UpdatedAt = now
        };

        _db.Projects.Add(project);

        // 5. Người tạo → Lead
        _db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = userId,
            Role = MemberRole.Lead,
            JoinedAt = now
        });

        await _db.SaveChangesAsync(ct);

        // 6. Response — mapping thủ công
        return new ProjectResponse(
            project.Id,
            project.InternshipId,
            project.Name,
            project.Description,
            project.Status.ToString(),
            project.StartDate,
            project.EndDate,
            project.CreatedBy,
            project.CreatedAt,
            project.UpdatedAt);
    }
}
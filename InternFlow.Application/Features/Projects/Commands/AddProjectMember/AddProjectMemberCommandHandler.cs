using AutoMapper;
using InternFlow.Application.Common.Caching;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Application.Features.Tasks.Queries.GetTaskSummary;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Projects.Commands.AddProjectMember;

public sealed class AddProjectMemberCommandHandler
    : IRequestHandler<AddProjectMemberCommand, ProjectMemberResponse>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IInternshipQueryService _internshipQuery;

    public AddProjectMemberCommandHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser,
        IMapper mapper,
        IMediator mediator,
        IInternshipQueryService internshipQuery)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
        _mediator = mediator;
        _internshipQuery = internshipQuery;
    }

    public async Task<ProjectMemberResponse> Handle(
        AddProjectMemberCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();
        var isAdmin = _currentUser.IsInRole(UserRole.Admin.ToString());

        await _db.EnsureCanManageProjectAsync(
            cmd.ProjectId, userId, _currentUser.Role, ct);

        var project = await _db.Projects
            .AsNoTracking()
            .FirstAsync(p => p.Id == cmd.ProjectId, ct);

        var targetUser = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == cmd.Request.UserId, ct)
            ?? throw new NotFoundException(
                $"User {cmd.Request.UserId} không tồn tại.");

        if (targetUser.Role != UserRole.Intern && targetUser.Role != UserRole.Mentor)
        {
            throw new ConflictException(
                "Chỉ Intern hoặc Mentor được thêm vào dự án.");
        }

        if (targetUser.Role == UserRole.Intern)
        {
            var inBatch = await _internshipQuery.IsInternInBatchAsync(
                cmd.Request.UserId, project.InternshipId, ct);

            if (!inBatch)
                throw new ConflictException("Intern không thuộc đợt thực tập của dự án.");

            if (!isAdmin)
            {
                var isMentorOfIntern = await _internshipQuery
                    .IsMentorAssignedToInternAsync(
                        userId, cmd.Request.UserId, project.InternshipId, ct);

                if (!isMentorOfIntern)
                    throw new ForbiddenException(
                        "Mentor chỉ được thêm intern mình phụ trách.");
            }
        }
        else if (targetUser.Role == UserRole.Mentor)
        {
            if (!isAdmin)
                throw new ForbiddenException(
                    "Chỉ Admin được thêm Mentor vào dự án.");
        }

        // Chặn trùng member active
        var existing = await _db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == cmd.ProjectId
                                    && m.UserId == cmd.Request.UserId, ct);

        if (existing is not null && existing.RemovedAt is null)
            throw new ConflictException(
                "User đã là thành viên của dự án.");

        // Nếu từng bị xóa -> reactivate
        var now = DateTime.UtcNow;
        if (existing is not null)
        {
            existing.RemovedAt = null;
            existing.JoinedAt = now;
            existing.Role = cmd.Request.Role;
        }
        else
        {
            _db.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = cmd.ProjectId,
                UserId = cmd.Request.UserId,
                Role = cmd.Request.Role,
                JoinedAt = now
            });
        }

        await _db.SaveChangesAsync(ct);
        await _mediator.Publish(
            new CacheInvalidationNotification(
                CacheKeys.ForProject<GetTaskSummaryQuery>(cmd.ProjectId)),
            ct);

        var createdMember = await _db.ProjectMembers
        .Include(m => m.User)
        .AsNoTracking()
        .FirstAsync(m => m.ProjectId == cmd.ProjectId
                    && m.UserId == cmd.Request.UserId, ct);

        return _mapper.Map<ProjectMemberResponse>(createdMember);
    }
}

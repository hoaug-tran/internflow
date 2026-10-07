using AutoMapper;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Projects.DTOs;
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
    private readonly IInternshipQueryService _internshipQuery;

    public AddProjectMemberCommandHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser,
        IMapper mapper,
        IInternshipQueryService internshipQuery)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
        _internshipQuery = internshipQuery;
    }

    public async Task<ProjectMemberResponse> Handle(
        AddProjectMemberCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.EnsureRole(UserRole.Admin, UserRole.Mentor);
        var isAdmin = _currentUser.IsInRole(UserRole.Admin.ToString());

        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == cmd.ProjectId, ct)
            ?? throw new NotFoundException(
                $"Dự án {cmd.ProjectId} không tồn tại.");
        // Chỉ Admin hoặc Lead của dự án mới thêm member
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
                    "Chỉ Admin hoặc Lead của dự án mới thêm thành viên.");
        }
        var targetUser = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == cmd.Request.UserId, ct)
            ?? throw new NotFoundException(
                $"User {cmd.Request.UserId} không tồn tại.");

        if (targetUser.Role.ToString() != nameof(UserRole.Intern)
            && targetUser.Role.ToString() != nameof(UserRole.Mentor))
        {
            throw new ConflictException(
                "Chỉ Intern hoặc Mentor được thêm vào dự án.");
        }
        //Nếu là intern thì kiểm tra cùng đợt
        if (targetUser.Role.ToString() == nameof(UserRole.Intern))
        {
            var inBatch = await _internshipQuery.IsInternInBatchAsync(
                cmd.Request.UserId, project.InternshipId, ct);

            if (!inBatch)
                throw new ConflictException(
                    "Intern không thuộc đợt thực tập của dự án.");

            //Nếu là Mentor -> check có đang phụ trách intern không
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

        var createdMember = await _db.ProjectMembers
    .Include(m => m.User)
    .AsNoTracking()
    .FirstAsync(m => m.ProjectId == cmd.ProjectId
                  && m.UserId == cmd.Request.UserId, ct);

        return _mapper.Map<ProjectMemberResponse>(createdMember);
    }
}

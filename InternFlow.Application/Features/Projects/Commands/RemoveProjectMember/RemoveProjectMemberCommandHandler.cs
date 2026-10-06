using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using TaskStatus = InternFlow.Domain.Projects.TaskStatus;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Projects.Commands.RemoveProjectMember;

public sealed class RemoveProjectMemberCommandHandler
    : IRequestHandler<RemoveProjectMemberCommand, Unit>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public RemoveProjectMemberCommandHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(
        RemoveProjectMemberCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.EnsureRole(UserRole.Admin, UserRole.Mentor);
        var isAdmin = _currentUser.IsInRole(UserRole.Admin.ToString());

        var project = await _db.Projects
            .AsNoTracking()
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
                    "Chỉ Admin hoặc Lead mới xóa thành viên.");
        }

        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == cmd.ProjectId
                                    && m.UserId == cmd.UserId, ct)
            ?? throw new NotFoundException(
                "Thành viên không tồn tại trong dự án.");

        if (member.RemovedAt is not null)
            throw new ConflictException(
                "Thành viên đã bị xóa trước đó.");

        if (member.Role == MemberRole.Lead)
        {
            var leadCount = await _db.ProjectMembers
                .CountAsync(m => m.ProjectId == cmd.ProjectId
                              && m.Role == MemberRole.Lead
                              && m.RemovedAt == null, ct);

            if (leadCount <= 1)
                throw new ConflictException(
                    "Không thể xóa Lead cuối cùng của dự án.");
        }

        // Chặn nếu member còn task chưa Done
        var hasOpenTasks = await _db.Tasks
            .AsNoTracking()
            .AnyAsync(t => t.ProjectId == cmd.ProjectId
                        && t.AssigneeId == cmd.UserId
                        && t.Status != TaskStatus.Done
                        && t.Status != TaskStatus.Cancelled, ct);

        if (hasOpenTasks)
            throw new ConflictException(
                "User còn task chưa hoàn thành. Cần reassign trước khi xóa.");

        member.RemovedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return Unit.Value;
    }
}

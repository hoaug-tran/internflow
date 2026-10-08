using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using TaskStatus = InternFlow.Domain.Projects.TaskStatus;
using MediatR;
using Microsoft.EntityFrameworkCore;
using InternFlow.Application.Common.Caching;
using InternFlow.Application.Features.Tasks.Queries.GetTaskSummary;
using InternFlow.Application.Features.Projects.Queries.GetProjectMembers;

namespace InternFlow.Application.Features.Projects.Commands.RemoveProjectMember;

public sealed class RemoveProjectMemberCommandHandler
    : IRequestHandler<RemoveProjectMemberCommand, Unit>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMediator _mediator;

    public RemoveProjectMemberCommandHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser,
        IMediator mediator)
    {
        _db = db;
        _currentUser = currentUser;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(
        RemoveProjectMemberCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();

        await _db.EnsureCanManageProjectAsync(
           cmd.ProjectId, userId, _currentUser.Role, ct);

        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == cmd.ProjectId, ct)
            ?? throw new NotFoundException(
                $"Dự án {cmd.ProjectId} không tồn tại.");

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
        await _mediator.Publish(
    new CacheInvalidationNotification(
        CacheKeys.ForProject<GetProjectMembersQuery>(cmd.ProjectId)),
    ct);

        await _mediator.Publish(
            new CacheInvalidationNotification(
                CacheKeys.ForProject<GetTaskSummaryQuery>(cmd.ProjectId)),
            ct);

        return Unit.Value;
    }
}

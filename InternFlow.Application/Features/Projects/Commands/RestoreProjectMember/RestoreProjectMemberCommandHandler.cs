using AutoMapper;
using InternFlow.Application.Common.Caching;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Projects.DTOs;
using InternFlow.Application.Features.Projects.Queries.GetProjectMembers;
using InternFlow.Application.Features.Tasks.Queries.GetTaskSummary;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Projects.Commands.RestoreProjectMember;

public sealed class RestoreProjectMemberCommandHandler
    : IRequestHandler<RestoreProjectMemberCommand, ProjectMemberResponse>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _uow;

    public RestoreProjectMemberCommandHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser,
        IMapper mapper,
        IMediator mediator,
        IUnitOfWork uow)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
        _mediator = mediator;
        _uow = uow;
    }

    public async Task<ProjectMemberResponse> Handle(
        RestoreProjectMemberCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();

        await _db.EnsureCanManageProjectAsync(
            cmd.ProjectId, userId, _currentUser.Role, ct);

        var member = await _db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == cmd.ProjectId
                                    && m.UserId == cmd.UserId, ct)
            ?? throw new NotFoundException(
                "Thành viên không tồn tại trong dự án.");

        if (member.RemovedAt is null)
            throw new ConflictException(
                "Thành viên chưa bị xóa, không cần khôi phục.");

        member.RemovedAt = null;
        member.JoinedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        await _mediator.Publish(
            new CacheInvalidationNotification(
                CacheKeys.ForProject<GetProjectMembersQuery>(cmd.ProjectId)),
            ct);

        await _mediator.Publish(
            new CacheInvalidationNotification(
                CacheKeys.ForProject<GetTaskSummaryQuery>(cmd.ProjectId)),
            ct);

        var restored = await _db.ProjectMembers
            .Include(m => m.User)
            .AsNoTracking()
            .FirstAsync(m => m.ProjectId == cmd.ProjectId
                          && m.UserId == cmd.UserId, ct);

        return _mapper.Map<ProjectMemberResponse>(restored);
    }
}

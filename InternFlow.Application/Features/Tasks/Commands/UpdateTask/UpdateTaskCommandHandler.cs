using AutoMapper;
using InternFlow.Application.Common.Caching;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Application.Features.Tasks.Queries.GetTaskSummary;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Tasks.Commands.UpdateTask;

public sealed class UpdateTaskCommandHandler
    : IRequestHandler<UpdateTaskCommand, TaskResponse>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public UpdateTaskCommandHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser,
        IMapper mapper,
        IMediator mediator)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<TaskResponse> Handle(
        UpdateTaskCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.EnsureRole(UserRole.Admin, UserRole.Mentor);
        var isAdmin = _currentUser.IsInRole(UserRole.Admin.ToString());

        // Load task with Project
        var task = await _db.Tasks
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == cmd.TaskId, ct)
            ?? throw new NotFoundException(
                $"Task {cmd.TaskId} không tồn tại.");

        if (!isAdmin)
        {
            var inProject = task.Project.CreatedBy == userId
                || await _db.ProjectMembers.AnyAsync(m =>
                    m.ProjectId == task.ProjectId
                    && m.UserId == userId
                    && m.RemovedAt == null, ct);

            if (!inProject)
                throw new NotFoundException(
                    $"Task {cmd.TaskId} không tồn tại.");
        }

        // 4. Nếu đổi assignee → validate assignee mới
        if (task.AssigneeId != cmd.Request.AssigneeId)
        {
            await EnsureValidAssigneeAsync(
                cmd.Request.AssigneeId, task.ProjectId, ct);
        }

        // 5. Update fields — AutoMapper (skip ProjectId, Status)
        _mapper.Map(cmd.Request, task);
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        await _mediator.Publish(
            new CacheInvalidationNotification(
                CacheKeys.ForProject<GetTaskSummaryQuery>(task.ProjectId)), ct);

        // Load lại với Assignee để map response
        var updatedTask = await _db.Tasks
            .Include(t => t.Assignee)
            .AsNoTracking()
            .FirstAsync(t => t.Id == task.Id, ct);

        return _mapper.Map<TaskResponse>(updatedTask);
    }

    private async Task EnsureValidAssigneeAsync(
        Guid assigneeId, Guid projectId, CancellationToken ct)
    {

        var assignee = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == assigneeId)
            .Select(u => new { u.Id, u.Role, u.IsActive })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(
                $"User {assigneeId} không tồn tại.");

        if (assignee.Role != UserRole.Intern)
            throw new ConflictException("Chỉ Intern được giao task.");

        if (!assignee.IsActive)
            throw new ConflictException("Intern đã bị vô hiệu hóa.");

        // Assignee phải là member active của project
        var isMember = await _db.ProjectMembers
            .AsNoTracking()
            .AnyAsync(m => m.ProjectId == projectId
                        && m.UserId == assigneeId
                        && m.RemovedAt == null, ct);

        if (!isMember)
            throw new ConflictException(
                "Intern không phải thành viên của dự án.");
    }
}

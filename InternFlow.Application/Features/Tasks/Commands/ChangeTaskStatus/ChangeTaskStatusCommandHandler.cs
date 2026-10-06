using AutoMapper;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Domain.Tasks;
using InternFlow.Shared.Exceptions;
using TaskStatus = InternFlow.Domain.Projects.TaskStatus;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Tasks.Commands.ChangeTaskStatus;

public sealed class ChangeTaskStatusCommandHandler
    : IRequestHandler<ChangeTaskStatusCommand, TaskResponse>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public ChangeTaskStatusCommandHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<TaskResponse> Handle(
        ChangeTaskStatusCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();
        var role = Enum.Parse<UserRole>(_currentUser.Role ?? string.Empty);

        var task = await _db.Tasks
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .FirstOrDefaultAsync(t => t.Id == cmd.TaskId, ct)
            ?? throw new NotFoundException(
                $"Task {cmd.TaskId} không tồn tại.");

        // Check scope - Intern chỉ được thao tác task của mình
        await EnsureTaskInScopeAsync(task, userId, role, ct);

        // State machine — TryTransition
        var transition = TaskWorkflow.TryTransition(
            task.Status, cmd.NewStatus, role);

        if (!transition.IsSuccess)
        {
            throw transition.FailureReason switch
            {
                TransitionFailureReason.InvalidTransition =>
                    new ConflictException(transition.ErrorMessage!),
                TransitionFailureReason.Forbidden =>
                    new ForbiddenException(transition.ErrorMessage!),
                _ => new InvalidOperationException(
                    "Unexpected transition failure.")
            };
        }

        //Áp dụng thay đổi
        var oldStatus = task.Status;
        task.Status = cmd.NewStatus;
        task.UpdatedAt = DateTime.UtcNow;

        if (cmd.NewStatus == TaskStatus.Done)
        {
            task.CompletedAt = DateTime.UtcNow;
        }
        else if (oldStatus == TaskStatus.Done && cmd.NewStatus != TaskStatus.Done)
        {
            task.CompletedAt = null;
        }

        await _db.SaveChangesAsync(ct);

        // #T14 Xóa cache task-summary:{projectId}
        // await _cache.RemoveAsync($"task-summary:{task.ProjectId}", ct);

        return _mapper.Map<TaskResponse>(task);
    }

    /// <summary>
    /// Kiểm tra task có nằm trong phạm vi của user không.
    /// </summary>
    private async Task EnsureTaskInScopeAsync(
        TaskItem task, Guid userId, UserRole role, CancellationToken ct)
    {
        switch (role)
        {
            case UserRole.Admin:
                return;

            case UserRole.Mentor:
                // Mentor: task thuộc project mình là Lead hoặc Member
                var isMentorInProject = task.Project.CreatedBy == userId
                    || await _db.ProjectMembers.AnyAsync(m =>
                        m.ProjectId == task.ProjectId
                        && m.UserId == userId
                        && m.RemovedAt == null, ct);

                if (!isMentorInProject)
                    throw new NotFoundException(
                        $"Task {task.Id} không tồn tại.");

                return;

            case UserRole.Intern:
                // Intern: chỉ task được assign cho mình
                if (task.AssigneeId != userId)
                    throw new NotFoundException(
                        $"Task {task.Id} không tồn tại.");

                return;

            default:
                throw new ForbiddenException("Vai trò không hợp lệ.");
        }
    }
}

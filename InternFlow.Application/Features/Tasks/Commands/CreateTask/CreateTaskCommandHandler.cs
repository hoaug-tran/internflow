using AutoMapper;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using TaskStatus = InternFlow.Domain.Projects.TaskStatus;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using InternFlow.Application.Features.Tasks.Queries.GetTaskSummary;
using InternFlow.Application.Common.Caching;

namespace InternFlow.Application.Features.Tasks.Commands.CreateTask;

public sealed class CreateTaskCommandHandler
    : IRequestHandler<CreateTaskCommand, TaskResponse>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IInternshipQueryService _internshipQuery;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public CreateTaskCommandHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser,
        IInternshipQueryService internshipQuery,
        IMapper mapper,
        IMediator mediator)
    {
        _db = db;
        _currentUser = currentUser;
        _internshipQuery = internshipQuery;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<TaskResponse> Handle(
        CreateTaskCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();

        await _db.EnsureCanManageProjectAsync(
            cmd.ProjectId, userId, _currentUser.Role, ct);

        var project = await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == cmd.ProjectId)
            .Select(p => new { p.Id, p.InternshipId })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(
                $"Dự án {cmd.ProjectId} không tồn tại.");

        // Assignee phải là Intern, active
        var assignee = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == cmd.Request.AssigneeId)
            .Select(u => new { u.Id, u.Role, u.IsActive })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(
                $"User {cmd.Request.AssigneeId} không tồn tại.");

        if (assignee.Role != UserRole.Intern)
            throw new ConflictException("Chỉ Intern được giao task.");

        if (!assignee.IsActive)
            throw new ConflictException("Intern đã bị vô hiệu hóa.");

        // Assignee phải là member active của project
        var isMember = await _db.ProjectMembers
            .AsNoTracking()
            .AnyAsync(m => m.ProjectId == cmd.ProjectId
                        && m.UserId == cmd.Request.AssigneeId
                        && m.RemovedAt == null, ct);

        if (!isMember)
            throw new ConflictException(
                "Intern không phải thành viên của dự án.");

        //Intern phải thuộc đợt của project
        var inBatch = await _internshipQuery.IsInternInBatchAsync(
            cmd.Request.AssigneeId, project.InternshipId, ct);

        if (!inBatch)
            throw new ConflictException(
                "Intern không thuộc đợt thực tập của dự án.");

        var task = _mapper.Map<TaskItem>(cmd.Request);
        var now = DateTime.UtcNow;

        task.Id = Guid.NewGuid();
        task.ProjectId = cmd.ProjectId;
        task.Status = TaskStatus.Todo;
        task.CreatedBy = userId;
        task.CreatedAt = now;
        task.UpdatedAt = now;
        task.CompletedAt = null;

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(ct);
        await _mediator.Publish(
            new CacheInvalidationNotification(
                CacheKeys.ForProject<GetTaskSummaryQuery>(task.ProjectId)),
    ct);

        var createdTask = await _db.Tasks
            .Include(t => t.Assignee)
            .AsNoTracking()
            .FirstAsync(t => t.Id == task.Id, ct);

        return _mapper.Map<TaskResponse>(createdTask);
    }
}

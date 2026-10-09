using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Tasks.DTOs;
using TaskStatus = InternFlow.Domain.Projects.TaskStatus;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Tasks.Queries.GetTaskSummary;

public sealed class GetTaskSummaryHandler
    : IRequestHandler<GetTaskSummaryQuery, TaskSummaryResponse>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    public GetTaskSummaryHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TaskSummaryResponse> Handle(
        GetTaskSummaryQuery request,
        CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();
        var role = _currentUser.Role;
        await _db.EnsureCanManageProjectAsync(request.ProjectId, userId, role, ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var tasks = await _db.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == request.ProjectId)
            .Select(t => new
            {
                t.Status,
                t.DueDate,
                t.AssigneeId,
                AssigneeName = t.Assignee != null ? t.Assignee.FullName : null
            })
            .ToListAsync(ct);

        int Count(TaskStatus s) => tasks.Count(t => t.Status == s);

        bool IsOverdue(DateOnly? due) =>
            due.HasValue && due.Value < today;

        bool IsOpenOverdue(TaskStatus status, DateOnly? due) =>
            IsOverdue(due)
            && status != TaskStatus.Done
            && status != TaskStatus.Cancelled;

        var byAssignee = tasks
            .Where(t => t.AssigneeId.HasValue)
            .GroupBy(t => t.AssigneeId!.Value)
            .Select(g => new TaskSummaryByAssignee(
                UserId: g.Key,
                FullName: g.First().AssigneeName ?? "(unknown)",
                Total: g.Count(),
                Done: g.Count(t => t.Status == TaskStatus.Done),
                InProgress: g.Count(t => t.Status == TaskStatus.InProgress),
                Overdue: g.Count(t => IsOpenOverdue(t.Status, t.DueDate))))
            .OrderByDescending(x => x.Total)
            .ToList();

        return new TaskSummaryResponse(
            ProjectId: request.ProjectId,
            TotalTasks: tasks.Count,
            TodoCount: Count(TaskStatus.Todo),
            InProgressCount: Count(TaskStatus.InProgress),
            InReviewCount: Count(TaskStatus.InReview),
            DoneCount: Count(TaskStatus.Done),
            CancelledCount: Count(TaskStatus.Cancelled),
            OverdueCount: tasks.Count(t => IsOpenOverdue(t.Status, t.DueDate)),
            ByAssignee: byAssignee,
            GeneratedAtUtc: DateTime.UtcNow);
    }
}

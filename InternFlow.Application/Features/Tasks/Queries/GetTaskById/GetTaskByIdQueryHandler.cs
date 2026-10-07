using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Tasks.Queries.GetTaskById;

public sealed class GetTaskByIdQueryHandler
    : IRequestHandler<GetTaskByIdQuery, TaskResponse>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetTaskByIdQueryHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TaskResponse> Handle(
        GetTaskByIdQuery req, CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();
        var role = _currentUser.Role;

        var task = await _db.Tasks
            .AsNoTracking()
            .Where(t => t.Id == req.TaskId)
            .Select(t => new
            {
                t.Id,
                t.ProjectId,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.AssigneeId,
                AssigneeName = t.Assignee != null ? t.Assignee.FullName : null,
                t.DueDate,
                t.CompletedAt,
                t.CreatedBy,
                t.CreatedAt,
                t.UpdatedAt,
                ProjectCreatedBy = t.Project.CreatedBy
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(
                $"Task {req.TaskId} không tồn tại.");

        var inScope = role switch
        {
            nameof(UserRole.Admin) => true,
            nameof(UserRole.Intern) => task.AssigneeId == userId,
            nameof(UserRole.Mentor) => task.ProjectCreatedBy == userId
                || await _db.ProjectMembers.AnyAsync(m =>
                    m.ProjectId == task.ProjectId
                    && m.UserId == userId
                    && m.RemovedAt == null, ct),
            _ => false
        };

        if (!inScope)
            throw new NotFoundException($"Task {req.TaskId} không tồn tại.");

        return new TaskResponse
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            AssigneeId = task.AssigneeId,
            AssigneeName = task.AssigneeName,
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt,
            CreatedBy = task.CreatedBy,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
}

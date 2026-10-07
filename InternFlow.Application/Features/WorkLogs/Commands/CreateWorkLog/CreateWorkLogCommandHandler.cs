using AutoMapper;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.WorkLogs.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using TaskStatus = InternFlow.Domain.Projects.TaskStatus;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.WorkLogs.Commands.CreateWorkLog;

public sealed class CreateWorkLogCommandHandler
    : IRequestHandler<CreateWorkLogCommand, WorkLogResponse>
{
    private const int MaxHoursPerDay = 12;
    private const int DoneGracePeriodDays = 3;

    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IInternshipQueryService _internshipQuery;
    private readonly IMapper _mapper;

    public CreateWorkLogCommandHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser,
        IInternshipQueryService internshipQuery,
        IMapper mapper)
    {
        _db = db;
        _currentUser = currentUser;
        _internshipQuery = internshipQuery;
        _mapper = mapper;
    }

    public async Task<WorkLogResponse> Handle(
        CreateWorkLogCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.EnsureRole(UserRole.Intern);

        // Load task với Include(Project) để có InternshipId
        var task = await _db.Tasks
            .Include(t => t.Project)
            .AsNoTracking()
            .Where(t => t.Id == cmd.Request.TaskItemId)
            .Select(t => new
            {
                t.Id,
                t.AssigneeId,
                t.Status,
                t.ProjectId,
                InternshipId = t.Project.InternshipId,
                ProjectStatus = t.Project.Status,
                t.CompletedAt
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(
                $"Task {cmd.Request.TaskItemId} không tồn tại.");

        // Ownership
        if (task.AssigneeId != userId)
            throw new ForbiddenException(
                "Chỉ được ghi worklog cho task của mình.");

        // Không ghi cho Cancelled
        if (task.Status == TaskStatus.Cancelled)
            throw new ConflictException(
                "Không thể ghi worklog cho task đã hủy.");

        if (task.Status == TaskStatus.Todo)
            throw new ConflictException(
                "Không thể ghi worklog cho task chưa bắt đầu (Todo).");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (cmd.Request.WorkDate > today)
            throw new BadRequestException("WorkDate không được ở tương lai.");

        if (task.ProjectStatus == ProjectStatus.OnHold
        || task.ProjectStatus == ProjectStatus.Cancelled)
            throw new ConflictException(
                "Không thể ghi worklog cho dự án đã đóng/hủy.");

        var isActive = await _internshipQuery.IsInternActiveInBatchAsync(
        userId, task.InternshipId, ct);
        if (!isActive)
            throw new ConflictException(
                "Intern không còn hoạt động trong đợt này.");

        var inBatch = await _internshipQuery.IsWorkDateInBatchAsync(
            task.InternshipId, cmd.Request.WorkDate, ct);

        if (!inBatch)
            throw new ConflictException(
                "WorkDate không nằm trong thời gian đợt thực tập.");

        // Task Done -> ghi bù 3 ngày
        if (task.Status == TaskStatus.Done && task.CompletedAt.HasValue)
        {
            var daysSinceDone = (DateTime.UtcNow.Date
                - task.CompletedAt.Value.Date).TotalDays;
            if (daysSinceDone > DoneGracePeriodDays)
                throw new ConflictException(
                    $"Task đã Done quá {DoneGracePeriodDays} ngày — không thể ghi bù.");
        }

        var totalHours = await _db.WorkLogs
            .Where(w => w.UserId == userId
                     && w.WorkDate == cmd.Request.WorkDate
                     && w.DeletedAt == null)
            .SumAsync(w => w.Hours, ct);

        if (totalHours + cmd.Request.Hours > MaxHoursPerDay)
            throw new ConflictException(
                $"Tổng giờ trong ngày vượt {MaxHoursPerDay}.");

        var exists = await _db.WorkLogs
            .AnyAsync(w => w.TaskItemId == cmd.Request.TaskItemId
                        && w.UserId == userId
                        && w.WorkDate == cmd.Request.WorkDate
                        && w.DeletedAt == null, ct);

        if (exists)
            throw new ConflictException(
                "Đã có worklog cho task này vào ngày này.");

        var workLog = _mapper.Map<WorkLog>(cmd.Request);
        var now = DateTime.UtcNow;
        workLog.Id = Guid.NewGuid();
        workLog.UserId = userId;
        workLog.CreatedAt = now;
        workLog.UpdatedAt = now;

        _db.WorkLogs.Add(workLog);
        await _db.SaveChangesAsync(ct);

        // Load lại với navigation
        var created = await _db.WorkLogs
            .Include(w => w.TaskItem)
            .Include(w => w.User)
            .AsNoTracking()
            .FirstAsync(w => w.Id == workLog.Id, ct);

        return _mapper.Map<WorkLogResponse>(created);
    }
}

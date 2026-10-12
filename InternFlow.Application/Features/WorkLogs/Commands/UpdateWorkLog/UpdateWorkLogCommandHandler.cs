using AutoMapper;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.WorkLogs.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.WorkLogs.Commands.UpdateWorkLog;

public sealed class UpdateWorkLogCommandHandler
    : IRequestHandler<UpdateWorkLogCommand, WorkLogResponse>
{
    private const int EditWindowDays = 7;
    private const int MaxHoursPerDay = 12;

    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;

    public UpdateWorkLogCommandHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser,
        IMapper mapper,
        IUnitOfWork uow)
    {
        _db = db;
        _currentUser = currentUser;
        _mapper = mapper;
        _uow = uow;
    }

    public async Task<WorkLogResponse> Handle(
        UpdateWorkLogCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.EnsureRole(UserRole.Intern);

        var workLog = await _db.WorkLogs
            .FirstOrDefaultAsync(w => w.Id == cmd.WorkLogId
                                   && w.DeletedAt == null, ct)
            ?? throw new NotFoundException(
                $"WorkLog {cmd.WorkLogId} không tồn tại.");

        // Owner
        if (workLog.UserId != userId)
            throw new NotFoundException(
                $"WorkLog {cmd.WorkLogId} không tồn tại.");

        // Trong 7 ngày
        if ((DateTime.UtcNow - workLog.CreatedAt).TotalDays > EditWindowDays)
            throw new ConflictException(
                $"Chỉ được sửa worklog trong {EditWindowDays} ngày.");

        // Tổng giờ/ngày (trừ bản ghi hiện tại)
        var totalOther = await _db.WorkLogs
            .Where(w => w.UserId == userId
                     && w.WorkDate == cmd.Request.WorkDate
                     && w.Id != workLog.Id
                     && w.DeletedAt == null)
            .SumAsync(w => w.Hours, ct);

        if (totalOther + cmd.Request.Hours > MaxHoursPerDay)
            throw new ConflictException(
                $"Tổng giờ trong ngày vượt {MaxHoursPerDay}.");

        workLog.WorkDate = cmd.Request.WorkDate;
        workLog.Hours = cmd.Request.Hours;
        workLog.Description = cmd.Request.Description;
        workLog.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        var updated = await _db.WorkLogs
            .Include(w => w.TaskItem)
            .Include(w => w.User)
            .AsNoTracking()
            .FirstAsync(w => w.Id == workLog.Id, ct);

        return _mapper.Map<WorkLogResponse>(updated);
    }
}

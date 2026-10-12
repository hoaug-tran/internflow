using AutoMapper;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.WorkLogs.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.WorkLogs.Commands.RestoreWorkLog;

public sealed class RestoreWorkLogCommandHandler
    : IRequestHandler<RestoreWorkLogCommand, WorkLogResponse>
{
    private const int RestoreWindowDays = 7;

    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _uow;

    public RestoreWorkLogCommandHandler(
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
        RestoreWorkLogCommand cmd, CancellationToken ct)
    {
        var userId = _currentUser.EnsureRole(UserRole.Intern);

        var workLog = await _db.WorkLogs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(w => w.Id == cmd.Id && w.DeletedAt != null, ct)
            ?? throw new NotFoundException(
                $"Worklog {cmd.Id} không tồn tại hoặc chưa bị xóa.");

        if (workLog.UserId != userId)
            throw new NotFoundException(
                $"Worklog {cmd.Id} không tồn tại.");

        var daysSinceDeleted = (DateTime.UtcNow - workLog.DeletedAt!.Value).TotalDays;
        if (daysSinceDeleted > RestoreWindowDays)
            throw new ConflictException(
                $"Chỉ có thể khôi phục worklog trong vòng {RestoreWindowDays} ngày.");

        workLog.DeletedAt = null;
        workLog.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(ct);

        var restored = await _db.WorkLogs
            .Include(w => w.TaskItem)
            .Include(w => w.User)
            .AsNoTracking()
            .FirstAsync(w => w.Id == workLog.Id, ct);

        return _mapper.Map<WorkLogResponse>(restored);
    }
}

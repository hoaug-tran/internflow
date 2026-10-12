using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.WorkLogs.Commands.DeleteWorkLog;

public sealed class DeleteWorkLogCommandHandler
    : IRequestHandler<DeleteWorkLogCommand, Unit>
{
    private const int UpdateWindowDays = 7;

    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _uow;

    public DeleteWorkLogCommandHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser,
        IUnitOfWork uow)
    {
        _db = db;
        _currentUser = currentUser;
        _uow = uow;
    }

    public async Task<Unit> Handle(
        DeleteWorkLogCommand request, CancellationToken ct)
    {
        var userId = _currentUser.EnsureRole(UserRole.Intern);

        var workLog = await _db.WorkLogs
            .FirstOrDefaultAsync(
                w => w.Id == request.Id && w.DeletedAt == null, ct)
            ?? throw new NotFoundException(
                $"Worklog {request.Id} không tồn tại.");

        // Chỉ owner
        if (workLog.UserId != userId)
            throw new NotFoundException(
                $"Worklog {request.Id} không tồn tại.");

        // Trong vòng 7 ngày
        var daysSinceCreated = (DateTime.UtcNow - workLog.CreatedAt).TotalDays;
        if (daysSinceCreated > UpdateWindowDays)
            throw new ConflictException(
                $"Chỉ có thể xóa worklog trong vòng {UpdateWindowDays} ngày.");

        workLog.DeletedAt = DateTime.UtcNow;
        workLog.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(ct);

        return Unit.Value;
    }
}

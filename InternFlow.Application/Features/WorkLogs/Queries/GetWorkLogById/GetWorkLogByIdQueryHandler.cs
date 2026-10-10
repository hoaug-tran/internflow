using AutoMapper;
using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.WorkLogs.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.WorkLogs.Queries.GetWorkLogById;

public sealed class GetWorkLogByIdQueryHandler
    : IRequestHandler<GetWorkLogByIdQuery, WorkLogResponse>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IInternshipQueryService _internshipQuery;
    private readonly IMapper _mapper;

    public GetWorkLogByIdQueryHandler(
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
        GetWorkLogByIdQuery request, CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();
        var role = _currentUser.Role;

        var workLog = await _db.WorkLogs
            .AsNoTracking()
            .Where(w => w.Id == request.Id && w.DeletedAt == null)
            .Select(w => new
            {
                w.Id,
                w.UserId,
                w.TaskItemId,
                w.WorkDate,
                w.Hours,
                w.Description,
                w.CreatedAt,
                w.UpdatedAt
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException(
                $"Worklog {request.Id} không tồn tại.");

        // ── Scope check ──
        if (IsRole(role, UserRole.Admin))
        {
            // Admin: pass
        }
        else if (IsRole(role, UserRole.Intern))
        {
            // Intern: chỉ worklog của mình
            if (workLog.UserId != userId)
                throw new NotFoundException(
                    $"Worklog {request.Id} không tồn tại.");
        }
        else if (IsRole(role, UserRole.Mentor))
        {
            // Mentor: chỉ worklog của intern mình phụ trách
            var interns = await _internshipQuery.GetInternsOfMentorAsync(userId, ct);
            if (!interns.Contains(workLog.UserId))
                throw new NotFoundException(
                    $"Worklog {request.Id} không tồn tại.");
        }
        else
        {
            throw new ForbiddenException("Vai trò không hợp lệ.");
        }

        // Load lại với navigation để map response đầy đủ
        var full = await _db.WorkLogs
            .AsNoTracking()
            .Include(w => w.TaskItem)
            .Include(w => w.User)
            .FirstAsync(w => w.Id == request.Id, ct);

        return _mapper.Map<WorkLogResponse>(full);
    }

    private static bool IsRole(string? role, UserRole expected)
        => string.Equals(role, expected.ToString(), StringComparison.OrdinalIgnoreCase);
}

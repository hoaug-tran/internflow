using InternFlow.Application.Common.Extensions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Extensions;
using InternFlow.Application.Features.WorkLogs.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.WorkLogs.Queries.GetWorkLogs;

public sealed class GetWorkLogsQueryHandler
    : IRequestHandler<GetWorkLogsQuery, PagedResult<WorkLogListItemResponse>>
{
    private readonly IInternFlowDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IInternshipQueryService _internshipQuery;

    public GetWorkLogsQueryHandler(
        IInternFlowDbContext db,
        ICurrentUserService currentUser,
        IInternshipQueryService internshipQuery)
    {
        _db = db;
        _currentUser = currentUser;
        _internshipQuery = internshipQuery;
    }

    public async Task<PagedResult<WorkLogListItemResponse>> Handle(
        GetWorkLogsQuery req, CancellationToken ct)
    {
        var userId = _currentUser.EnsureAuthenticated();
        var role = _currentUser.Role;
        var p = req.Parameters;

        var query = _db.WorkLogs
            .AsNoTracking()
            .Where(w => w.DeletedAt == null);

        if (role == nameof(UserRole.Intern))
        {
            query = query.Where(w => w.UserId == userId);
        }
        else if (role == nameof(UserRole.Mentor))
        {
            var interns = await _internshipQuery
                .GetInternsOfMentorAsync(userId, ct);

            query = query.Where(w => interns.Contains(w.UserId));
        }

        if (p.TaskItemId.HasValue)
            query = query.Where(w => w.TaskItemId == p.TaskItemId.Value);

        if (p.ProjectId.HasValue)
            query = query.Where(w => w.TaskItem.ProjectId == p.ProjectId.Value);

        if (p.UserId.HasValue)
            query = query.Where(w => w.UserId == p.UserId.Value);

        if (p.From.HasValue)
            query = query.Where(w => w.WorkDate >= p.From.Value);

        if (p.To.HasValue)
            query = query.Where(w => w.WorkDate <= p.To.Value);

        query = query.OrderByDescending(w => w.WorkDate).ThenBy(w => w.Id);

        var dtoQuery = query.Select(w => new WorkLogListItemResponse(
            w.Id,
            w.TaskItemId,
            w.TaskItem.Title,
            w.UserId,
            w.User.FullName,
            w.WorkDate,
            w.Hours,
            w.Description,
            w.CreatedAt));

        return await dtoQuery.ToPagedResultAsync(p, ct);
    }
}

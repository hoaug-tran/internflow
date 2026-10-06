using InternFlow.Application.Common.Interfaces;
using InternFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using InternFlow.Domain.Internships;

namespace InternFlow.Infrastructure.Services;

public class InternshipQueryService : IInternshipQueryService
{
    private readonly InternFlowDbContext _context;

    public InternshipQueryService(InternFlowDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Phục vụ cho Quang: Đảm bảo Mentor A đang thực sự hướng dẫn Intern B trong đợt (EndedAt IS NULL)
    /// </summary>
    public async Task<bool> IsMentorAssignedToInternAsync(
        Guid mentorId,
        Guid internId,
        Guid batchId,
        CancellationToken ct = default)
    {
        return await _context.MentorAssignments
            .AsNoTracking()
            .AnyAsync(ma => ma.MentorId == mentorId
                         && ma.InternId == internId
                         && ma.InternshipId == batchId
                         && ma.EndedAt == null, ct);
    }

    /// <summary>
    /// Phục vụ cho Tú: Đảm bảo Intern được giao task đã tham gia vào đợt thực tập này
    /// </summary>
    public async Task<bool> IsInternInBatchAsync(
        Guid internId,
        Guid batchId,
        CancellationToken ct = default)
    {
        return await _context.BatchInterns
            .AsNoTracking()
            .AnyAsync(bi => bi.InternId == internId
                         && bi.InternshipId == batchId, ct);
    }

    public async Task<bool> IsWorkDateInBatchAsync(
        Guid internshipId,
        DateOnly workDate,
        CancellationToken ct = default)
    {
        return await _context.Internships
            .AsNoTracking()
            .AnyAsync(i => i.Id == internshipId
                        && i.StartDate <= workDate
                        && i.EndDate >= workDate, ct);
    }
    public async Task<IReadOnlyList<Guid>> GetInternsOfMentorAsync(
        Guid mentorId,
        CancellationToken ct = default)
    {
        return await _context.MentorAssignments
            .AsNoTracking()
            .Where(ma => ma.MentorId == mentorId && ma.EndedAt == null)
            .Select(ma => ma.InternId)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<bool> IsInternActiveInBatchAsync(
    Guid internId, Guid batchId, CancellationToken ct = default)
    {
        return await _context.BatchInterns
            .AnyAsync(bi => bi.InternId == internId
                         && bi.InternshipId == batchId
                         && bi.Status == BatchInternStatus.InProgress
                         && bi.Intern.IsActive, ct);
    }
}

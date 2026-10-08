using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Evaluations;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Evaluations;

public static class EvaluationGuard
{
    public static void EnsurePeriodOpen(EvaluationPeriod period)
    {
        if (period.Status != PeriodStatus.Open)
            throw new ConflictException("Kỳ đánh giá chưa mở hoặc đã đóng.");
    }

    public static async Task EnsureCanEvaluateAsync(
        IInternFlowDbContext context, ICurrentUserService user,
        Guid internshipId, Guid internId, CancellationToken ct)
    {
        if (user.UserId is not { } userId)
            throw new ForbiddenException("Không xác định được người dùng hiện tại.");

        if (user.IsInRole(nameof(UserRole.Admin)))
        {
            var inBatch = await context.BatchInterns
                .AnyAsync(b => b.InternshipId == internshipId && b.InternId == internId, ct);
            if (!inBatch)
                throw new BadRequestException("Intern không thuộc đợt thực tập của kỳ đánh giá này.");
            return;
        }

        var assigned = await context.MentorAssignments.AnyAsync(m =>
            m.InternshipId == internshipId && m.InternId == internId &&
            m.MentorId == userId && m.EndedAt == null, ct);
        if (!assigned)
            throw new ForbiddenException("Bạn không phụ trách intern này.");
    }

    public static Task<List<Criterion>> LoadActiveCriteriaAsync(
        IInternFlowDbContext context, Guid periodId, CancellationToken ct) =>
        context.Criteria.AsNoTracking()
            .Where(c => c.EvaluationPeriodId == periodId && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);
}

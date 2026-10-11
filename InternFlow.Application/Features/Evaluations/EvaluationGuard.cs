using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Evaluations;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;

namespace InternFlow.Application.Features.Evaluations;

public static class EvaluationGuard
{
    public static void EnsurePeriodOpen(EvaluationPeriod period)
    {
        if (period.Status != PeriodStatus.Open)
            throw new ConflictException("Kỳ đánh giá chưa mở hoặc đã đóng.");
    }

    public static async Task EnsureCanEvaluateAsync(
        IInternshipQueryService internships, ICurrentUserService user,
        Guid internshipId, Guid internId, CancellationToken ct)
    {
        if (user.UserId is not { } userId)
            throw new ForbiddenException("Không xác định được người dùng hiện tại.");

        if (user.IsInRole(nameof(UserRole.Admin)))
        {
            if (!await internships.IsInternInBatchAsync(internId, internshipId, ct))
                throw new BadRequestException("Intern không thuộc đợt thực tập của kỳ đánh giá này.");
            return;
        }

        if (!await internships.IsMentorAssignedToInternAsync(userId, internId, internshipId, ct))
            throw new ForbiddenException("Bạn không phụ trách intern này.");
    }
}

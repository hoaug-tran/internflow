namespace InternFlow.Application.Common.Interfaces;

public interface IInternshipQueryService
{
    // Phục vụ cho Quang: Đảm bảo Mentor A không chấm điểm nhầm Intern của Mentor B
    Task<bool> IsMentorAssignedToInternAsync(Guid mentorId, Guid internId, Guid batchId, CancellationToken ct = default);

    // Phục vụ cho Tú: Đảm bảo Intern được giao Task thực sự có mặt trong đợt thực tập này
    Task<bool> IsInternInBatchAsync(Guid internId, Guid batchId, CancellationToken ct = default);
}
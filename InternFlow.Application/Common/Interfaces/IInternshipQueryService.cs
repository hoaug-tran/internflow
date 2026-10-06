namespace InternFlow.Application.Common.Interfaces;

public interface IInternshipQueryService
{
    // Phục vụ cho Quang: Đảm bảo Mentor A không chấm điểm nhầm Intern của Mentor B
    Task<bool> IsMentorAssignedToInternAsync(Guid mentorId, Guid internId, Guid batchId, CancellationToken ct = default);

    // Phục vụ cho Tú: Đảm bảo Intern được giao Task thực sự có mặt trong đợt thực tập này
    Task<bool> IsInternInBatchAsync(Guid internId, Guid batchId, CancellationToken ct = default);
    //Kiểm tra WorkDate có nằm trong khoảng đợt thực tập không. 
    Task<bool> IsWorkDateInBatchAsync(
        Guid internshipId,
        DateOnly workDate,
        CancellationToken ct = default);

    // Lấy danh sách intern mà mentor đang phụ trách (active)
    Task<IReadOnlyList<Guid>> GetInternsOfMentorAsync(
        Guid mentorId,
        CancellationToken ct = default);
    // Không ghi và Logs nếu intern rời đợt
    Task<bool> IsInternActiveInBatchAsync(
    Guid internId, Guid batchId, CancellationToken ct = default);
}

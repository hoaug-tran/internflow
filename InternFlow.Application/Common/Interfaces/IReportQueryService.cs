using InternFlow.Application.Features.Reports.DTOs;

namespace InternFlow.Application.Common.Interfaces;

public interface IReportQueryService
{
    Task<IReadOnlyList<TaskProgressRowDto>> GetTaskProgressAsync(
        Guid requesterId, bool isAdmin, Guid? projectId, CancellationToken ct = default);
    Task<IReadOnlyList<InternWorkloadRowDto>> GetInternWorkloadAsync(
    Guid requesterId, bool isAdmin, Guid? projectId, DateOnly? from, DateOnly? to,
    CancellationToken ct = default);

    Task<IReadOnlyList<MentorWorkloadRowDto>> GetMentorWorkloadAsync(
        Guid requesterId, bool isAdmin, Guid? projectId, DateOnly? from, DateOnly? to,
        CancellationToken ct = default);
}

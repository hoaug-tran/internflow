using InternFlow.Application.Features.Reports.DTOs;

namespace InternFlow.Application.Common.Interfaces;

public interface IReportQueryService
{
    Task<IReadOnlyList<TaskProgressRowDto>> GetTaskProgressAsync(
        Guid requesterId, bool isAdmin, Guid? projectId, CancellationToken ct = default);
}

using InternFlow.Domain.Evaluations;

namespace InternFlow.Application.Common.Interfaces.Repositories.Evaluations;

public interface IEvaluationRepository
{
    Task<Evaluation?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<Evaluation?> GetPublishedByIdForInternAsync(Guid id, Guid internId, CancellationToken ct = default);
    Task<List<Evaluation>> GetPublishedByInternAsync(Guid internId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid periodId, Guid internId, CancellationToken ct = default);
    Task<bool> AnyInPeriodAsync(Guid periodId, CancellationToken ct = default);
    Task<bool> IsCriterionUsedAsync(Guid criterionId, CancellationToken ct = default);
    void Add(Evaluation evaluation);
    void AddDetails(IEnumerable<EvaluationDetail> details);
}

using InternFlow.Domain.Evaluations;

namespace InternFlow.Application.Common.Interfaces.Repositories.Evaluations;

public interface ICriterionRepository
{
    Task<Criterion?> GetByIdAsync(Guid id, Guid periodId, CancellationToken ct = default);
    Task<decimal> GetActiveTotalWeightAsync(Guid periodId, Guid? excludeCriterionId = null, CancellationToken ct = default);
    Task<List<Criterion>> GetByPeriodAsync(Guid periodId, CancellationToken ct = default);
    Task<List<Criterion>> GetActiveByPeriodAsync(Guid periodId, CancellationToken ct = default);
    Task<List<Criterion>> GetActiveByPeriodsAsync(IReadOnlyCollection<Guid> periodIds, CancellationToken ct = default);
    void Add(Criterion criterion);
    void Remove(Criterion criterion);
    void RemoveRange(IEnumerable<Criterion> criteria);
}

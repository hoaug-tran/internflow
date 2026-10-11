using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Domain.Evaluations;
using InternFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Infrastructure.Repositories.Evaluations;

public class CriterionRepository(InternFlowDbContext db) : ICriterionRepository
{
    public Task<Criterion?> GetByIdAsync(Guid id, Guid periodId, CancellationToken ct = default)
        => db.Criteria.FirstOrDefaultAsync(c => c.Id == id && c.EvaluationPeriodId == periodId, ct);

    public Task<decimal> GetActiveTotalWeightAsync(Guid periodId, Guid? excludeCriterionId = null, CancellationToken ct = default)
        => db.Criteria
            .Where(c => c.EvaluationPeriodId == periodId && c.IsActive
                        && (excludeCriterionId == null || c.Id != excludeCriterionId))
            .SumAsync(c => c.Weight, ct);

    public Task<List<Criterion>> GetByPeriodAsync(Guid periodId, CancellationToken ct = default)
        => db.Criteria.AsNoTracking()
            .Where(c => c.EvaluationPeriodId == periodId)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);

    public Task<List<Criterion>> GetActiveByPeriodAsync(Guid periodId, CancellationToken ct = default)
        => db.Criteria.AsNoTracking()
            .Where(c => c.EvaluationPeriodId == periodId && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);

    public Task<List<Criterion>> GetActiveByPeriodsAsync(IReadOnlyCollection<Guid> periodIds, CancellationToken ct = default)
        => db.Criteria.AsNoTracking()
            .Where(c => periodIds.Contains(c.EvaluationPeriodId) && c.IsActive)
            .ToListAsync(ct);

    public void Add(Criterion criterion) => db.Criteria.Add(criterion);
    public void Remove(Criterion criterion) => db.Criteria.Remove(criterion);
    public void RemoveRange(IEnumerable<Criterion> criteria) => db.Criteria.RemoveRange(criteria);
}

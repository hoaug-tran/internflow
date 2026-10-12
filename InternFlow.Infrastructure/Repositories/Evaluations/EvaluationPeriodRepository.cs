using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Domain.Evaluations;
using InternFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Infrastructure.Repositories.Evaluations;

public class EvaluationPeriodRepository(InternFlowDbContext db) : IEvaluationPeriodRepository
{
    public Task<EvaluationPeriod?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.EvaluationPeriods.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<EvaluationPeriod?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default)
        => db.EvaluationPeriods.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<EvaluationPeriod?> GetByIdWithCriteriaAsync(Guid id, CancellationToken ct = default)
        => db.EvaluationPeriods.Include(p => p.Criteria).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => db.EvaluationPeriods.AnyAsync(p => p.Id == id, ct);

    public Task<bool> InternshipExistsAsync(Guid internshipId, CancellationToken ct = default)
        => db.Internships.AnyAsync(i => i.Id == internshipId, ct);

    public Task<List<EvaluationPeriod>> GetListAsync(Guid? internshipId, CancellationToken ct = default)
    {
        var q = db.EvaluationPeriods.AsNoTracking();
        if (internshipId.HasValue) q = q.Where(p => p.InternshipId == internshipId.Value);
        return q.OrderByDescending(p => p.StartDate).ToListAsync(ct);
    }

    public void Add(EvaluationPeriod period) => db.EvaluationPeriods.Add(period);
    public void Remove(EvaluationPeriod period) => db.EvaluationPeriods.Remove(period);
}

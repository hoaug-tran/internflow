using InternFlow.Application.Common.Interfaces.Repositories.Evaluations;
using InternFlow.Domain.Evaluations;
using InternFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Infrastructure.Repositories.Evaluations;

public class EvaluationRepository(InternFlowDbContext db) : IEvaluationRepository
{
    public Task<Evaluation?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => db.Evaluations.Include(e => e.Details).FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<Evaluation?> GetPublishedByIdForInternAsync(Guid id, Guid internId, CancellationToken ct = default)
        => db.Evaluations.AsNoTracking()
            .Include(e => e.Details)
            .FirstOrDefaultAsync(e => e.Id == id && e.InternId == internId
                                      && e.Status == EvaluationStatus.Published, ct);

    public Task<List<Evaluation>> GetPublishedByInternAsync(Guid internId, CancellationToken ct = default)
        => db.Evaluations.AsNoTracking()
            .Include(e => e.Details)
            .Include(e => e.EvaluationPeriod)
            .Where(e => e.InternId == internId && e.Status == EvaluationStatus.Published)
            .OrderByDescending(e => e.SubmittedAt)
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(Guid periodId, Guid internId, CancellationToken ct = default)
        => db.Evaluations.AnyAsync(e => e.EvaluationPeriodId == periodId && e.InternId == internId, ct);

    public Task<bool> AnyInPeriodAsync(Guid periodId, CancellationToken ct = default)
        => db.Evaluations.AnyAsync(e => e.EvaluationPeriodId == periodId, ct);

    public Task<bool> IsCriterionUsedAsync(Guid criterionId, CancellationToken ct = default)
        => db.EvaluationDetails.AnyAsync(d => d.CriterionId == criterionId, ct);

    public void Add(Evaluation evaluation) => db.Evaluations.Add(evaluation);
    public void AddDetails(IEnumerable<EvaluationDetail> details) => db.EvaluationDetails.AddRange(details);
}

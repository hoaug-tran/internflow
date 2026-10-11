using InternFlow.Domain.Evaluations;

namespace InternFlow.Application.Common.Interfaces.Repositories.Evaluations;

public interface IEvaluationPeriodRepository
{
    Task<EvaluationPeriod?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<EvaluationPeriod?> GetByIdNoTrackingAsync(Guid id, CancellationToken ct = default);
    Task<EvaluationPeriod?> GetByIdWithCriteriaAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> InternshipExistsAsync(Guid internshipId, CancellationToken ct = default);
    Task<List<EvaluationPeriod>> GetListAsync(Guid? internshipId, CancellationToken ct = default);
    void Add(EvaluationPeriod period);
    void Remove(EvaluationPeriod period);
}

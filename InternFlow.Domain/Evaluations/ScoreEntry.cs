namespace InternFlow.Domain.Evaluations;

public record ScoreEntry(Guid CriterionId, decimal Score, string? Comment);

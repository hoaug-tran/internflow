namespace InternFlow.Domain.Evaluations;

public enum EvaluationRule
{
    Conflict, // -> 409
    Invalid   // -> 400
}

public class EvaluationRuleException(EvaluationRule rule, string message) : Exception(message)
{
    public EvaluationRule Rule { get; } = rule;
}

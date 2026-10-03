namespace InternFlow.Domain.Tasks;

public enum TransitionFailureReason
{
    None,
    InvalidTransition,
    Forbidden
}
public sealed class TaskTransitionResult
{
    public bool IsSuccess { get; }
    public TransitionFailureReason FailureReason { get; }
    public string? ErrorMessage { get; }

    private TaskTransitionResult(
        bool isSuccess,
        TransitionFailureReason reason,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        FailureReason = reason;
        ErrorMessage = errorMessage;
    }

    public static TaskTransitionResult Success() =>
        new(true, TransitionFailureReason.None, null);

    public static TaskTransitionResult InvalidTransition(string message) =>
        new(false, TransitionFailureReason.InvalidTransition, message);

    public static TaskTransitionResult Forbidden(string message) =>
        new(false, TransitionFailureReason.Forbidden, message);
}

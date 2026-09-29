namespace InternFlow.Domain.Tasks;

/// <summary>
///   - InvalidTransition -> 409 Conflict (bản thân bước chuyển này không tồn tại,
///     bất kể ai gọi, ví dụ Todo -> Done bỏ qua các bước ở giữa)
///   - Forbidden          -> 403 Forbidden (bước chuyển tồn tại nhưng vai trò
///     đang gọi không được phép thực hiện, ví dụ Intern tự duyệt Review -> Done)
/// </summary>
public enum TransitionFailureReason
{
    None,
    InvalidTransition,
    Forbidden
}

/// <summary>
/// Kết quả của một lần thử chuyển trạng thái công việc.
/// Dùng thay vì ném exception ngay trong Domain, để tầng gọi (Application/Api)
/// tự quyết định cách phản hồi (mã lỗi, thông điệp) mà không phải bắt exception.
/// </summary>
public sealed class TaskTransitionResult
{
    public bool IsSuccess { get; }
    public TransitionFailureReason FailureReason { get; }
    public string? ErrorMessage { get; }

    private TaskTransitionResult(bool isSuccess, TransitionFailureReason reason, string? errorMessage)
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

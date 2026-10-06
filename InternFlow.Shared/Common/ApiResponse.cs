namespace InternFlow.Shared.Common;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyList<string>? Errors { get; init; }
    public string? TraceId { get; init; }

    public static ApiResponse<T> Ok(T data, string message = "Thành công")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, IReadOnlyList<string>? errors = null, string? traceId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new() { Success = false, Message = message, Errors = errors, TraceId = traceId };
    }

}
public class ApiResponse : ApiResponse<object>
{
    public static new ApiResponse Ok(object? data = null, string message = "Thành công")
        => new() { Success = true, Message = message, Data = data };

    public static new ApiResponse Fail(string message, IReadOnlyList<string>? errors = null, string? traceId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        return new() { Success = false, Message = message, Errors = errors, TraceId = traceId };
    }
}


using InternFlow.Domain.Enums;

namespace InternFlow.Domain.Tasks;

public static class TaskWorkflow
{
    private static readonly Dictionary<(TaskStatusValue From, TaskStatusValue To), UserRole[]> AllowedTransitions =
        new()
        {
            [(TaskStatusValue.Todo, TaskStatusValue.InProgress)] =
                new[] { UserRole.Intern, UserRole.Mentor, UserRole.Admin },

            [(TaskStatusValue.InProgress, TaskStatusValue.Review)] =
                new[] { UserRole.Intern, UserRole.Mentor, UserRole.Admin },

            [(TaskStatusValue.Review, TaskStatusValue.Done)] =
                new[] { UserRole.Mentor, UserRole.Admin },

            [(TaskStatusValue.Review, TaskStatusValue.InProgress)] =
                new[] { UserRole.Mentor, UserRole.Admin },

            [(TaskStatusValue.Todo, TaskStatusValue.Cancelled)] =
                new[] { UserRole.Mentor, UserRole.Admin },

            [(TaskStatusValue.InProgress, TaskStatusValue.Cancelled)] =
                new[] { UserRole.Mentor, UserRole.Admin },

            [(TaskStatusValue.Review, TaskStatusValue.Cancelled)] =
                new[] { UserRole.Mentor, UserRole.Admin },
        };

    /// <summary>
    /// Dùng để phân biệt lỗi 409 (bước chuyển không tồn tại) với lỗi 403 (đúng bước nhưng sai vai trò).
    /// </summary>
    public static bool IsKnownTransition(TaskStatusValue from, TaskStatusValue to) =>
        AllowedTransitions.ContainsKey((from, to));

    /// <summary>
    /// Vai trò `role` có được phép chuyển từ `from` sang `to` hay không.
    /// Trả về false nếu bước chuyển không tồn tại HOẶC vai trò không được phép.
    /// </summary>
    public static bool CanTransition(TaskStatusValue from, TaskStatusValue to, UserRole role) =>
        AllowedTransitions.TryGetValue((from, to), out var roles) && roles.Contains(role);

    /// <summary>
    /// Thử chuyển trạng thái, trả về kết quả có phân biệt rõ hai loại lỗi
    /// để tầng API map đúng mã lỗi (409 hay 403).
    /// </summary>
    public static TaskTransitionResult TryTransition(TaskStatusValue from, TaskStatusValue to, UserRole role)
    {
        if (!AllowedTransitions.TryGetValue((from, to), out var allowedRoles))
        {
            return TaskTransitionResult.InvalidTransition(
                $"Không thể chuyển công việc từ trạng thái {from} sang {to}.");
        }

        if (!allowedRoles.Contains(role))
        {
            return TaskTransitionResult.Forbidden(
                $"Vai trò {role} không được phép chuyển công việc từ {from} sang {to}.");
        }

        return TaskTransitionResult.Success();
    }

    /// <summary>
    /// Danh sách trạng thái mà `role` được phép chuyển tới, xuất phát từ `from`.
    /// </summary>
    public static IReadOnlyCollection<TaskStatusValue> GetAvailableNextStatuses(TaskStatusValue from, UserRole role) =>
        AllowedTransitions
            .Where(kv => kv.Key.From == from && kv.Value.Contains(role))
            .Select(kv => kv.Key.To)
            .ToList();
}

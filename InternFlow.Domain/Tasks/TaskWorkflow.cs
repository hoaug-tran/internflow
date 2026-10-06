using TaskStatusValue = InternFlow.Domain.Projects.TaskStatus;
using InternFlow.Domain.Identity;
namespace InternFlow.Domain.Tasks;

public static class TaskWorkflow
{
    private static readonly Dictionary<(TaskStatusValue From, TaskStatusValue To), UserRole[]> AllowedTransitions =
        new()
        {
            //Luồng chính: Todo → InProgress → InReview → Done 

            [(TaskStatusValue.Todo, TaskStatusValue.InProgress)] =
                new[] { UserRole.Intern, UserRole.Mentor, UserRole.Admin },

            [(TaskStatusValue.InProgress, TaskStatusValue.InReview)] =
                new[] { UserRole.Intern, UserRole.Mentor, UserRole.Admin },

            [(TaskStatusValue.InReview, TaskStatusValue.Done)] =
                new[] { UserRole.Mentor, UserRole.Admin },   // Intern khong duoc

            [(TaskStatusValue.InReview, TaskStatusValue.InProgress)] =
                new[] { UserRole.Mentor, UserRole.Admin },


            [(TaskStatusValue.Todo, TaskStatusValue.Cancelled)] =
                new[] { UserRole.Mentor, UserRole.Admin },

            [(TaskStatusValue.InProgress, TaskStatusValue.Cancelled)] =
                new[] { UserRole.Mentor, UserRole.Admin },

            [(TaskStatusValue.InReview, TaskStatusValue.Cancelled)] =
                new[] { UserRole.Mentor, UserRole.Admin },

        };

    /// <summary>
    /// Bước chuyển (from → to) có tồn tại trong quy trình không, không xét vai trò.
    /// Dùng phân biệt lỗi 409 lỗi 403
    /// </summary>
    public static bool IsKnownTransition(TaskStatusValue from, TaskStatusValue to) =>
        AllowedTransitions.ContainsKey((from, to));

    public static bool CanTransition(TaskStatusValue from, TaskStatusValue to, UserRole role) =>
        AllowedTransitions.TryGetValue((from, to), out var roles) && roles.Contains(role);

    /// <summary>
    /// Thử chuyển trạng thái, trả về kết quả phân biệt 2 lỗi 403, 409
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
    public static IReadOnlyCollection<TaskStatusValue> GetAvailableNextStatuses(TaskStatusValue from, UserRole role) =>
        AllowedTransitions
            .Where(kv => kv.Key.From == from && kv.Value.Contains(role))
            .Select(kv => kv.Key.To)
            .ToList();
}

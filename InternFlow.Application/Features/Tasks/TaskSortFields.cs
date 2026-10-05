namespace InternFlow.Application.Features.Tasks;

public static class TaskSortFields
{
    public const string DueDate = "dueDate";
    public const string Priority = "priority";
    public const string CreatedAt = "createdAt";
    public const string Status = "status";
    public const string Default = CreatedAt;

    public static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase)
        {
            DueDate, Priority, CreatedAt, Status
        };

    public static bool IsAllowed(string? sortBy) =>
        string.IsNullOrWhiteSpace(sortBy) || Allowed.Contains(sortBy);
}

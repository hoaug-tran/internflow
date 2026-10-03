namespace InternFlow.Application.Features.Projects;

public static class ProjectSortFields
{
    public const string Name = "name";
    public const string CreatedAt = "createdAt";
    public const string Status = "status";
    public const string Default = CreatedAt;

    private static readonly HashSet<string> AllowedSet = new(StringComparer.OrdinalIgnoreCase)
    {
        Name, CreatedAt, Status
    };
    public static IReadOnlyCollection<string> Allowed => AllowedSet;

    public static bool IsAllowed(string? sortBy) =>
        string.IsNullOrWhiteSpace(sortBy) || AllowedSet.Contains(sortBy);

}
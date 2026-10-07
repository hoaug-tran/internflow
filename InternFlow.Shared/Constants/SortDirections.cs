namespace InternFlow.Shared.Constants;

public static class SortDirections
{
    public const string Asc = "asc";
    public const string Desc = "desc";

    public static bool IsValid(string? dir) =>
        string.IsNullOrWhiteSpace(dir)
        || dir.Equals(Asc, StringComparison.OrdinalIgnoreCase)
        || dir.Equals(Desc, StringComparison.OrdinalIgnoreCase);

    public static bool IsDescending(string? dir) =>
        !string.IsNullOrWhiteSpace(dir)
        && dir.Equals(Desc, StringComparison.OrdinalIgnoreCase);
}

namespace InternFlow.Shared.Models;

public class PaginationQuery
{
    private const int MaxPageSize = 50;
    private const int DefaultPageSize = 20;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    public string? SortBy { get; set; }
    public string? SortDir { get; set; } = "asc";
    public string? Keyword { get; set; }

    public int Skip => (Page - 1) * PageSize;
}
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Extensions;
using InternFlow.Shared.Models;

namespace InternFlow.Infrastructure.Services;

public sealed class PagedQueryExecutor : IPagedQueryExecutor
{
    public Task<PagedResult<T>> ToPagedResultAsync<T>(
        IQueryable<T> query,
        PaginationQuery pagination,
        CancellationToken ct = default)
        => query.ToPagedResultAsync(pagination, ct);
}

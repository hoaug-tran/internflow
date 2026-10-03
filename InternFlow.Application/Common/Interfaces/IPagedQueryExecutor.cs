using InternFlow.Shared.Models;

namespace InternFlow.Application.Common.Interfaces;

public interface IPagedQueryExecutor
{
    Task<PagedResult<T>> ToPagedResultAsync<T>(
        IQueryable<T> query,
        PaginationQuery pagination,
        CancellationToken ct = default);
}

namespace InternFlow.Application.Common.Caching;

public interface ICacheKeyBuilder
{
    string Build<TRequest>(
        CacheableAttribute attribute,
        TRequest request,
        Guid? currentUserId);
}
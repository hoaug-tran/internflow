using InternFlow.Application.Common.Caching;

namespace InternFlow.Application.Common.Interfaces;

public interface ICacheKeyBuilder
{
    string Build<TRequest>(
        CacheableAttribute attribute,
        TRequest request,
        Guid? currentUserId);
}

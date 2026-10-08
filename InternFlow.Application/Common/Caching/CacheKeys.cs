using System.Reflection;

namespace InternFlow.Application.Common.Caching;

public static class CacheKeys
{
    public static string PrefixOf<TQuery>() where TQuery : notnull
    {
        var attr = typeof(TQuery).GetCustomAttribute<CacheableAttribute>()
            ?? throw new InvalidOperationException(
                $"{typeof(TQuery).Name} không gắn [Cacheable].");

        return attr.KeyPrefix;
    }

    /// <summary>Prefix có scope projectId — khớp DefaultCacheKeyBuilder (Guid "N").</summary>
    public static string ForProject<TQuery>(Guid projectId) where TQuery : notnull
        => $"{PrefixOf<TQuery>()}:{projectId:N}";

    /// <summary>Prefix có scope internshipId.</summary>
    public static string ForInternship<TQuery>(Guid internshipId) where TQuery : notnull
        => $"{PrefixOf<TQuery>()}:{internshipId:N}";

    /// <summary>Prefix có scope periodId (đánh giá).</summary>
    public static string ForPeriod<TQuery>(Guid periodId) where TQuery : notnull
        => $"{PrefixOf<TQuery>()}:{periodId:N}";
}
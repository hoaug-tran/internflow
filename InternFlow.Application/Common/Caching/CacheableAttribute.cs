namespace InternFlow.Application.Common.Caching;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CacheableAttribute : Attribute
{
    public string KeyPrefix { get; }
    public int TtlMinutes { get; }
    public bool ScopeToUser { get; init; }

    public CacheableAttribute(string keyPrefix, int ttlMinutes)
    {
        if (string.IsNullOrWhiteSpace(keyPrefix))
            throw new ArgumentException("Key prefix không được rỗng.", nameof(keyPrefix));
        if (ttlMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(ttlMinutes), "TTL phải > 0.");

        KeyPrefix = keyPrefix;
        TtlMinutes = ttlMinutes;
    }

    public TimeSpan Ttl => TimeSpan.FromMinutes(TtlMinutes);
}

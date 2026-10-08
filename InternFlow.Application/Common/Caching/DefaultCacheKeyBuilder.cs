using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace InternFlow.Application.Common.Caching;

public sealed class DefaultCacheKeyBuilder : ICacheKeyBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Build<TRequest>(
        CacheableAttribute attribute,
        TRequest request,
        Guid? currentUserId)
    {
        var suffix = BuildSuffix(request);
        var userPart = attribute.ScopeToUser ? $":u:{currentUserId}" : string.Empty;

        return string.IsNullOrEmpty(suffix)
            ? $"{attribute.KeyPrefix}{userPart}"
            : $"{attribute.KeyPrefix}{userPart}:{suffix}";
    }

    private static string BuildSuffix<TRequest>(TRequest request)
    {
        if (request is null) return string.Empty;

        var props = typeof(TRequest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ToArray();

        if (props.Length == 0) return string.Empty;

        // 1 property kiểu Guid/int/string ngắn -> key thân thiện
        if (props.Length == 1)
        {
            var value = props[0].GetValue(request);
            switch (value)
            {
                case Guid g: return g.ToString("N");
                case int or long or short: return value.ToString()!;
                case string s when s.Length <= 32: return s;
            }
        }

        // Nhiều property -> hash JSON
        var payload = JsonSerializer.Serialize(request, JsonOptions);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }
}
using System.Collections.Concurrent;
using InternFlow.Application.Common.Interfaces;

namespace InternFlow.Tests.Integration;

public sealed class FakeCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, object?> _store = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (_store.TryGetValue(key, out var value) && value is T typed)
            return Task.FromResult<T?>(typed);
        return Task.FromResult<T?>(default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        _store[key] = value;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        var keysToRemove = _store.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
            .ToList();

        foreach (var key in keysToRemove)
            _store.TryRemove(key, out _);

        return Task.CompletedTask;
    }

    public bool HasKey(string key) => _store.ContainsKey(key);
    public int Count => _store.Count;
    public void Clear() => _store.Clear();
}
using System.Reflection;
using InternFlow.Application.Common.Caching;
using InternFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InternFlow.Application.Common.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cache;
    private readonly ICacheKeyBuilder _keyBuilder;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly CacheableAttribute? _attribute;

    public CachingBehavior(
        ICacheService cache,
        ICacheKeyBuilder keyBuilder,
        ICurrentUserService currentUser,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _keyBuilder = keyBuilder;
        _currentUser = currentUser;
        _logger = logger;
        _attribute = typeof(TRequest).GetCustomAttribute<CacheableAttribute>();
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (_attribute is null)
            return await next();

        if (IsCommandType())
        {
            _logger.LogWarning(
                "[Cache] {Request} gắn [Cacheable] nhưng là command — bỏ qua cache.",
                typeof(TRequest).Name);
            return await next();
        }

        var key = _keyBuilder.Build(_attribute, request, _currentUser.UserId);

        // GET
        try
        {
            var cached = await _cache.GetAsync<TResponse>(key, ct);
            if (cached is not null)
            {
                _logger.LogInformation("[Cache] HIT {Key}", key);
                return cached;
            }
            _logger.LogInformation("[Cache] MISS {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Cache] GET lỗi {Key}, fallback DB", key);
        }

        var response = await next();

        // SET — không cache null để tránh cache âm
        if (response is not null)
        {
            try
            {
                await _cache.SetAsync(key, response, _attribute.Ttl, ct);
                _logger.LogInformation("[Cache] SET {Key} TTL={Ttl}", key, _attribute.Ttl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Cache] SET lỗi {Key}", key);
            }
        }

        return response;
    }

    private static bool IsCommandType()
    {
        var t = typeof(TResponse);
        return t == typeof(Unit)
            || t == typeof(bool)
            || t == typeof(int)
            || t == typeof(Guid);
    }
}

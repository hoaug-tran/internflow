using InternFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InternFlow.Application.Common.Caching;

public sealed class CacheInvalidationHandler
    : INotificationHandler<CacheInvalidationNotification>
{
    private readonly ICacheService _cache;
    private readonly ILogger<CacheInvalidationHandler> _logger;

    public CacheInvalidationHandler(
        ICacheService cache,
        ILogger<CacheInvalidationHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task Handle(
        CacheInvalidationNotification notification,
        CancellationToken ct)
    {
        try
        {
            await _cache.RemoveByPrefixAsync(notification.Prefix, ct);
            _logger.LogInformation(
                "[Cache] INVALIDATE prefix={Prefix}", notification.Prefix);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "[Cache] INVALIDATE lỗi prefix={Prefix}", notification.Prefix);
        }
    }
}
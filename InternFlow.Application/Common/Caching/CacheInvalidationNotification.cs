using MediatR;

namespace InternFlow.Application.Common.Caching;

public sealed record CacheInvalidationNotification(string Prefix) : INotification;

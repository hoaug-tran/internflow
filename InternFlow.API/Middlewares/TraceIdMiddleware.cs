using Serilog.Context;

namespace InternFlow.API.Middlewares;

public class TraceIdMiddleware
{
    public const string HeaderName = "X-Trace-Id";
    private readonly RequestDelegate _next;

    public TraceIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(traceId) || traceId.Length > 64)
            traceId = Guid.NewGuid().ToString("N");

        context.TraceIdentifier = traceId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = traceId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("TraceId", traceId))
        {
            await _next(context);
        }
    }
}

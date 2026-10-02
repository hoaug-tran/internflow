using System.Security.Claims;

namespace InternFlow.API.Extensions;

public static class HttpContextExtensions
{
    public static string GetIpAddress(this HttpContext context)
    {
        // 1. Kiểm tra xem có đi qua Proxy/Load Balancer/Docker không
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedHeader))
        {
            var ip = forwardedHeader.ToString().Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(ip))
            {
                return ip;
            }
        }

        // 2. Nếu không có proxy, lấy IP kết nối trực tiếp (chuyển về IPv4 cho dễ nhìn)
        return context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
    }

    public static string GetUserAgent(this HttpContext context)
    {
        return context.Request.Headers.UserAgent.ToString();
    }

    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? principal.FindFirst("sub")?.Value;

        if (Guid.TryParse(idClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}

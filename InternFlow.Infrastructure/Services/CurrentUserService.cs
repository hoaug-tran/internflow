using System.Security.Claims;
using InternFlow.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace InternFlow.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var v = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User?.FindFirstValue("sub");

            return Guid.TryParse(v, out var id) ? id : null;
        }
    }

    public string? Email =>
        User?.FindFirstValue(ClaimTypes.Email)
        ?? User?.FindFirstValue("email");

    public string? Role =>
        User?.FindFirstValue(ClaimTypes.Role)
        ?? User?.FindFirstValue("role");

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public bool IsInRole(string role)
    {
        if (User is null) return false;

        return User.IsInRole(role)
            || string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
    }
}
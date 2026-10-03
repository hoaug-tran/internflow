using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Exceptions;

namespace InternFlow.Application.Common.Extensions;

public static class CurrentUserExtensions
{
    public static Guid EnsureAuthenticated(this ICurrentUserService user)
    {
        if (!user.IsAuthenticated || user.UserId is null)
            throw new ForbiddenException("Chưa đăng nhập.");

        return user.UserId.Value;
    }
    public static Guid EnsureRole(
        this ICurrentUserService user,
        params UserRole[] allowedRoles)
    {
        var userId = user.EnsureAuthenticated();

        var role = user.Role;
        if (role is null || !allowedRoles.Any(r =>
            string.Equals(r.ToString(), role, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ForbiddenException(
                $"Chỉ {string.Join("/", allowedRoles)} được thực hiện.");
        }

        return userId;
    }
}
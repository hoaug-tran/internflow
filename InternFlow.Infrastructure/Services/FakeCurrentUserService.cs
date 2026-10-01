using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Identity;

namespace InternFlow.Infrastructure.Services;

public class FakeCurrentUserService : ICurrentUserService
{
    // Giả lập ID của một Mentor mẫu (khớp với seed data sau này)
    public Guid? UserId => Guid.Parse("11111111-1111-1111-1111-111111111111");

    public string? Email => "mentor.demo@internflow.com";

    // Dùng hằng số chuỗi UserRoles.Mentor thay vì Enum UserRole
    public string? Role => UserRole.Mentor.ToString();

    public bool IsAuthenticated => true;

    // So sánh chuỗi không phân biệt hoa thường
    public bool IsInRole(string role) =>
        string.Equals(role, UserRole.Mentor.ToString(), StringComparison.OrdinalIgnoreCase);
}

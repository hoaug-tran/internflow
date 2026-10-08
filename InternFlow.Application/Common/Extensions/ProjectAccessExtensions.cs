using InternFlow.Application.Common.Interfaces;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Common.Extensions;

public static class ProjectAccessExtensions
{
    // Quyền xem — Admin hoặc Member
    /// <summary>
    /// Đảm bảo user được phép XEM project.
    /// </summary>
    public static async Task EnsureCanViewProjectAsync(
        this IInternFlowDbContext db,
        Guid projectId,
        Guid userId,
        string? role,
        CancellationToken ct = default)
    {
        await db.EnsureProjectExistsAsync(projectId, ct);

        if (IsAdmin(role))
            return;

        if (!await db.CanViewProjectAsync(projectId, userId, ct))
            throw new NotFoundException($"Dự án {projectId} không tồn tại.");
    }

    /// <summary>
    /// Kiểm tra user có phải member active của project không.
    /// </summary>
    public static Task<bool> CanViewProjectAsync(
        this IInternFlowDbContext db,
        Guid projectId,
        Guid userId,
        CancellationToken ct = default)
        => db.IsProjectMemberAsync(projectId, userId, role: null, ct);
    // Quyền quản lý - Admin hoặc Lead

    /// <summary>
    /// Đảm bảo user được phép QUẢN LÝ project (sửa, thêm/xóa member, tạo task).
    /// </summary>
    public static async Task EnsureCanManageProjectAsync(
        this IInternFlowDbContext db,
        Guid projectId,
        Guid userId,
        string? role,
        CancellationToken ct = default)
    {
        await db.EnsureProjectExistsAsync(projectId, ct);

        if (IsAdmin(role))
            return;

        if (!await db.CanManageProjectAsync(projectId, userId, ct))
            throw new ForbiddenException(
                "Chỉ Admin hoặc Lead của dự án mới thao tác được.");
    }

    /// <summary>
    /// Kiểm tra user có phải Lead của project không.
    /// </summary>
    public static Task<bool> CanManageProjectAsync(
        this IInternFlowDbContext db,
        Guid projectId,
        Guid userId,
        CancellationToken ct = default)
        => db.IsProjectMemberAsync(projectId, userId, MemberRole.Lead, ct);

    /// <summary>
    /// Thu hẹp danh sách project theo quyền:
    ///   - Admin  : tất cả
    ///   - Mentor : project mình tạo HOẶC mình là member
    ///   - Intern : project mình là member
    ///   - Khác   : rỗng
    /// </summary>
    public static IQueryable<Project> ApplyProjectVisibilityScope(
        this IQueryable<Project> query,
        Guid userId,
        string? role)
    {
        if (IsAdmin(role))
            return query;

        if (IsRole(role, UserRole.Mentor))
        {
            return query.Where(p => p.CreatedBy == userId
                || p.Members.Any(m => m.UserId == userId && m.RemovedAt == null));
        }

        if (IsRole(role, UserRole.Intern))
        {
            return query.Where(p =>
                p.Members.Any(m => m.UserId == userId && m.RemovedAt == null));
        }
        return query.Where(_ => false);
    }
    private static async Task EnsureProjectExistsAsync(
        this IInternFlowDbContext db,
        Guid projectId,
        CancellationToken ct)
    {
        var exists = await db.Projects
            .AsNoTracking()
            .AnyAsync(p => p.Id == projectId, ct);

        if (!exists)
            throw new NotFoundException($"Dự án {projectId} không tồn tại.");
    }
    private static Task<bool> IsProjectMemberAsync(
        this IInternFlowDbContext db,
        Guid projectId,
        Guid userId,
        MemberRole? role,
        CancellationToken ct)
        => db.ProjectMembers
             .AsNoTracking()
             .AnyAsync(m => m.ProjectId == projectId
                         && m.UserId == userId
                         && m.RemovedAt == null
                         && (role == null || m.Role == role), ct);
    private static bool IsRole(string? role, UserRole expected)
        => string.Equals(role, expected.ToString(), StringComparison.OrdinalIgnoreCase);

    private static bool IsAdmin(string? role)
        => IsRole(role, UserRole.Admin);
}
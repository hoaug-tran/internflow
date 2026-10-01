using InternFlow.Application.Common.Interfaces;
using InternFlow.Infrastructure.Data;
using InternFlow.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.API.Extensions;

/// <summary>
/// Extension method đăng ký toàn bộ service của dự án.
/// Trong Program.cs chỉ cần gọi: builder.Services.AddProjectServices(builder.Configuration)
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddProjectServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ─── Database ────────────────────────────────────────────────────────
        services.AddDbContext<InternFlowDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Database=InternFlowDb;Trusted_Connection=True;TrustServerCertificate=True;";
            options.UseSqlServer(connectionString);
        });

        // ─── Application Services ────────────────────────────────────────────
        services.AddScoped<ICurrentUserService, FakeCurrentUserService>();
        services.AddScoped<IInternshipQueryService, InternshipQueryService>();

        return services;
    }
}

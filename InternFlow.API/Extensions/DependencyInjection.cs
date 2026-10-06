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
        // ─── 1. Database ────────────────────────────────────────────────────────
        services.AddDbContext<InternFlowDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=localhost;Database=InternFlowDb;Trusted_Connection=True;TrustServerCertificate=True;";
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IInternFlowDbContext>(sp => sp.GetRequiredService<InternFlowDbContext>());

        // ─── 2. MediatR (CQRS) ──────────────────────────────────────────────────
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(IInternFlowDbContext).Assembly);
        });

        // ─── 3. Application & Infrastructure Services ───────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IInternshipQueryService, InternshipQueryService>();

        // ─── 4. Health Checks ───────────────────────────────────────────────────
        services.AddHealthChecks()
            .AddDbContextCheck<InternFlowDbContext>("database", tags: new[] { "ready" });

        return services;
    }
}

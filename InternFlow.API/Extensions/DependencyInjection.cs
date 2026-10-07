using FluentValidation;
using InternFlow.Application.Common.Behaviors;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Projects.Commands.CreateProject;
using InternFlow.Application.Features.Projects.Queries.GetProjects;
using InternFlow.Infrastructure.Data;
using InternFlow.Infrastructure.Services;
using MediatR;
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
        // FluentValidation
        services.AddValidatorsFromAssembly(
            typeof(CreateProjectCommandValidator).Assembly,
            includeInternalTypes: true);
        services.AddAutoMapper(cfg => { }, typeof(CreateProjectCommandValidator).Assembly);
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));


        // ─── 3. Application & Infrastructure Services ───────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IInternshipQueryService, InternshipQueryService>();
<<<<<<< HEAD
        services.AddStackExchangeRedisCache(options =>
        {
            var redis = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.ConfigurationOptions = StackExchange.Redis.ConfigurationOptions.Parse(redis);
            options.ConfigurationOptions.AbortOnConnectFail = false;
            options.ConfigurationOptions.ConnectTimeout = 1000;
            options.ConfigurationOptions.SyncTimeout = 1000;
            options.ConfigurationOptions.AsyncTimeout = 1000;
        });
        services.AddSingleton<ICacheService, RedisCacheService>();
=======
        services.AddScoped<IPagedQueryExecutor, PagedQueryExecutor>();

>>>>>>> origin/main

        // ─── 4. Health Checks ───────────────────────────────────────────────────
        services.AddHealthChecks()
            .AddDbContextCheck<InternFlowDbContext>("database", tags: new[] { "ready" });

        return services;
    }
}

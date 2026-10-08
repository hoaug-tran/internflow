using FluentValidation;
using InternFlow.Application.Common.Behaviors;
using InternFlow.Application.Common.Caching;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Projects.Commands.CreateProject;
using InternFlow.Infrastructure.Data;
using InternFlow.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

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
            typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));


        // ─── 3. Application & Infrastructure Services ───────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IInternshipQueryService, InternshipQueryService>();
        services.AddScoped<IReportQueryService, ReportQueryService>();

        var redis = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddStackExchangeRedisCache(options =>
        {

            options.ConfigurationOptions = ConfigurationOptions.Parse(redis);
            options.ConfigurationOptions.AbortOnConnectFail = false;
            options.ConfigurationOptions.ConnectTimeout = 1000;
            options.ConfigurationOptions.SyncTimeout = 1000;
            options.ConfigurationOptions.AsyncTimeout = 1000;
        });

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(redis);
            options.AbortOnConnectFail = false;
            options.ConnectTimeout = 1000;
            options.SyncTimeout = 1000;
            options.AsyncTimeout = 1000;
            return ConnectionMultiplexer.Connect(options);
        });
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();

        services.AddScoped<IPagedQueryExecutor, PagedQueryExecutor>();


        // ─── 4. Health Checks ───────────────────────────────────────────────────
        services.AddHealthChecks()
            .AddDbContextCheck<InternFlowDbContext>("database", tags: new[] { "ready" });

        return services;
    }
}

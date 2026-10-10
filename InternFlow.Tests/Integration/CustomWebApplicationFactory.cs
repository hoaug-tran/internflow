using InternFlow.Application.Common.Interfaces;
using InternFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Testcontainers.MsSql;

namespace InternFlow.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _sqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 1. Bỏ DbContext đăng ký thật
            services.RemoveAll<DbContextOptions<InternFlowDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<InternFlowDbContext>();

            // 2. SQL Server container
            services.AddDbContext<InternFlowDbContext>(options =>
                options.UseSqlServer(_sqlContainer.GetConnectionString()));

            // 3. Override ICacheService
            services.RemoveAll<ICacheService>();
            services.AddSingleton<FakeCacheService>();
            services.AddSingleton<ICacheService>(sp =>
                sp.GetRequiredService<FakeCacheService>());

            // 4. Vô hiệu hóa Serilog cho test
            services.RemoveAll<ILoggerFactory>();
            services.AddSingleton<ILoggerFactory, NullLoggerFactory>();

            services.RemoveAll(typeof(ILogger<>));
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        Log.CloseAndFlush();
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();
        db.Database.Migrate();
        DevSeeder.SeedAsync(db).GetAwaiter().GetResult();

        return host;
    }
}

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<CustomWebApplicationFactory> { }

using InternFlow.Application.Common.Interfaces;
using InternFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InternFlow.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<InternFlowDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var dbInterfaceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IInternFlowDbContext));
            if (dbInterfaceDescriptor != null)
            {
                services.Remove(dbInterfaceDescriptor);
            }

            // Register InMemory database
            services.AddDbContext<InternFlowDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });

            services.AddScoped<IInternFlowDbContext>(sp => sp.GetRequiredService<InternFlowDbContext>());
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();
        db.Database.EnsureCreated();
        DatabaseSeeder.SeedAsync(db).GetAwaiter().GetResult();

        return host;
    }
}

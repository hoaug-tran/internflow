using InternFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Xunit;

namespace InternFlow.Tests.Infrastructure;

public class SqlServerTestBase : IAsyncLifetime
{
    // Khai báo container SQL Server 2022
    private readonly MsSqlContainer _msSqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();
    protected ApplicationDbContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // Khởi động container tạm thời
        await _msSqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_msSqlContainer.GetConnectionString(), b =>
                b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .Options;

        Context = new ApplicationDbContext(options);

        // Tự động chạy tất cả Migrations vào container SQL Server này
        await Context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
        // Xóa container sau khi test xong
        await _msSqlContainer.DisposeAsync();
    }
}

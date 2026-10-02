using InternFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.API.Extensions;

public static class DatabaseExtensions
{
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();
        await db.Database.MigrateAsync();
        await DevSeeder.SeedAsync(db);
    }
}

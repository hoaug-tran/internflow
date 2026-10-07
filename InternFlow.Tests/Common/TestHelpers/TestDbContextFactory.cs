using InternFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Tests.Common.TestHelpers;

public static class TestDbContextFactory
{
    public static InternFlowDbContext Create()
    {
        var options = new DbContextOptionsBuilder<InternFlowDbContext>()
            .UseInMemoryDatabase(databaseName: $"Test_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .Options;

        return new InternFlowDbContext(options);
    }
}

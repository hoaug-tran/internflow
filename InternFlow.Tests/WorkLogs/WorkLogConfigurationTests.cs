using FluentAssertions;
using InternFlow.Domain.Projects;
using InternFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InternFlow.Tests.WorkLogs;

public class WorkLogConfigurationTests : IDisposable
{
    private readonly InternFlowDbContext _db;

    public WorkLogConfigurationTests()
    {
        var options = new DbContextOptionsBuilder<InternFlowDbContext>()
            .UseInMemoryDatabase(databaseName: $"WorkLogTest_{Guid.NewGuid()}")
            .Options;
        _db = new InternFlowDbContext(options);
    }

    [Fact]
    public void WorkLog_Has_Renamed_Properties()
    {
        var entityType = _db.Model.FindEntityType(typeof(WorkLog));

        entityType!.FindProperty(nameof(WorkLog.TaskItemId)).Should().NotBeNull();
        entityType.FindProperty(nameof(WorkLog.UserId)).Should().NotBeNull();
        entityType.FindProperty(nameof(WorkLog.Hours)).Should().NotBeNull();
        entityType.FindProperty(nameof(WorkLog.Description)).Should().NotBeNull();
    }

    [Fact]
    public void WorkLog_DeletedAt_IsNullable()
    {
        var entityType = _db.Model.FindEntityType(typeof(WorkLog));
        var deletedAtProperty = entityType!.FindProperty(nameof(WorkLog.DeletedAt));

        deletedAtProperty.Should().NotBeNull();
        deletedAtProperty!.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void WorkLog_RowVersion_IsConcurrencyToken()
    {
        var entityType = _db.Model.FindEntityType(typeof(WorkLog));
        var rvProperty = entityType!.FindProperty(nameof(WorkLog.RowVersion));

        rvProperty.Should().NotBeNull();
        rvProperty!.IsConcurrencyToken.Should().BeTrue();
    }

    [Fact]
    public void WorkLog_Has_Unique_Filtered_Index()
    {
        var entityType = _db.Model.FindEntityType(typeof(WorkLog));
        var index = entityType!.GetIndexes()
            .FirstOrDefault(i => i.GetDatabaseName() == "UX_WorkLogs_Task_User_Date_Active");

        index.Should().NotBeNull();
        index!.IsUnique.Should().BeTrue();
        index.GetFilter().Should().Be("[DeletedAt] IS NULL");
    }

    [Fact]
    public void WorkLog_Has_CheckConstraint_On_Hours()
    {
        var entityType = _db.Model.FindEntityType(typeof(WorkLog));
        var checks = entityType!.GetCheckConstraints();

        checks.Should().Contain(c => c.Name == "CK_WorkLogs_Hours");
    }

    [Fact]
    public void WorkLog_Fk_To_TaskItem_Is_Restrict()
    {
        var entityType = _db.Model.FindEntityType(typeof(WorkLog));
        var fk = entityType!.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(TaskItem));

        fk.Should().NotBeNull();
        fk!.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
    }

    public void Dispose() => _db.Dispose();
}

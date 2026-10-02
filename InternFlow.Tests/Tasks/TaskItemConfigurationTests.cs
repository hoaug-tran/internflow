using FluentAssertions;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

using TaskStatus = InternFlow.Domain.Projects.TaskStatus;

namespace InternFlow.Tests.Tasks;

public class TaskItemConfigurationTests : IDisposable
{
    private readonly InternFlowDbContext _db;
    private readonly IModel _designTimeModel;

    public TaskItemConfigurationTests()
    {
        var options = new DbContextOptionsBuilder<InternFlowDbContext>()
            .UseInMemoryDatabase(databaseName: $"TaskItemTest_{Guid.NewGuid()}")
            .Options;
        _db = new InternFlowDbContext(options);

        _designTimeModel = _db.GetService<IDesignTimeModel>().Model;
    }



    [Fact]
    public void TaskStatus_Enum_Has_InReview_Not_Review()
    {
        Enum.GetNames<TaskStatus>().Should().Contain("InReview");
        Enum.GetNames<TaskStatus>().Should().NotContain("Review");
    }

    [Fact]
    public void TaskPriority_Enum_Has_4_Values()
    {
        Enum.GetNames<TaskPriority>().Should().BeEquivalentTo(
            new[] { "Low", "Medium", "High", "Critical" });
    }


    [Fact]
    public void Task_Status_Is_Configured()
    {
        var entityType = _designTimeModel.FindEntityType(typeof(TaskItem));
        var statusProperty = entityType!.FindProperty(nameof(TaskItem.Status));

        statusProperty.Should().NotBeNull();
        statusProperty!.GetMaxLength().Should().Be(20);
    }

    [Fact]
    public void Task_Priority_Is_Configured()
    {
        var entityType = _designTimeModel.FindEntityType(typeof(TaskItem));
        var priorityProperty = entityType!.FindProperty(nameof(TaskItem.Priority));

        priorityProperty.Should().NotBeNull();
        priorityProperty!.GetMaxLength().Should().Be(20);
    }

    [Fact]
    public void Task_AssigneeId_IsNullable()
    {
        var entityType = _designTimeModel.FindEntityType(typeof(TaskItem));
        var assigneeProperty = entityType!.FindProperty(nameof(TaskItem.AssigneeId));

        assigneeProperty.Should().NotBeNull();
        assigneeProperty!.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void Task_DueDate_IsNullable()
    {
        var entityType = _designTimeModel.FindEntityType(typeof(TaskItem));
        var dueDateProperty = entityType!.FindProperty(nameof(TaskItem.DueDate));

        dueDateProperty.Should().NotBeNull();
        dueDateProperty!.IsNullable.Should().BeTrue();
    }

    [Fact]
    public void Task_RowVersion_IsConcurrencyToken()
    {
        var entityType = _designTimeModel.FindEntityType(typeof(TaskItem));
        var rvProperty = entityType!.FindProperty(nameof(TaskItem.RowVersion));

        rvProperty.Should().NotBeNull();
        rvProperty!.IsConcurrencyToken.Should().BeTrue();
    }

    [Fact]
    public void Task_CreatedBy_Has_Fk_To_Users()
    {
        var entityType = _designTimeModel.FindEntityType(typeof(TaskItem));
        var fk = entityType!.GetForeignKeys()
            .FirstOrDefault(f => f.Properties.Any(p => p.Name == nameof(TaskItem.CreatedBy)));

        fk.Should().NotBeNull();
        fk!.PrincipalEntityType.ClrType.Should().Be<User>();
        fk.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
    }


    [Fact]
    public void Task_Has_CheckConstraint_On_Status()
    {
        var entityType = _designTimeModel.FindEntityType(typeof(TaskItem));
        var checks = entityType!.GetCheckConstraints();

        checks.Should().Contain(c => c.Name == "CK_Tasks_Status");
    }

    [Fact]
    public void Task_Has_CheckConstraint_On_Priority()
    {
        var entityType = _designTimeModel.FindEntityType(typeof(TaskItem));
        var checks = entityType!.GetCheckConstraints();

        checks.Should().Contain(c => c.Name == "CK_Tasks_Priority");
    }

    [Fact]
    public void Task_Has_CheckConstraint_On_CompletedAt()
    {
        var entityType = _designTimeModel.FindEntityType(typeof(TaskItem));
        var checks = entityType!.GetCheckConstraints();

        checks.Should().Contain(c => c.Name == "CK_Tasks_CompletedAt");
    }

    public void Dispose() => _db.Dispose();
}

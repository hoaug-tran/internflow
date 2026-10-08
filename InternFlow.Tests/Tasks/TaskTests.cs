using FluentAssertions;
using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Tasks.Commands.ChangeTaskStatus;
using InternFlow.Application.Features.Tasks.Commands.CreateTask;
using InternFlow.Application.Features.Tasks.Commands.UpdateTask;
using InternFlow.Application.Features.Tasks.DTOs;
using InternFlow.Application.Features.Tasks.Queries.GetTasks;
using TaskStatus = InternFlow.Domain.Projects.TaskStatus;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Projects;
using InternFlow.Infrastructure.Data;
using InternFlow.Shared.Exceptions;
using InternFlow.Tests.Common.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using InternFlow.Application.Features.Tasks;

namespace InternFlow.Tests.Features.Tasks;

/// <summary>
/// Test tổng hợp cho feature Tasks.
/// Gộp tất cả Command/Query test vào 1 file để dễ maintain.
/// </summary>
public class TaskTests
{
    #region Shared Helpers

    private static Mock<IInternshipQueryService> MockInternshipQuery(bool inBatch = true)
    {
        var m = new Mock<IInternshipQueryService>();
        m.Setup(x => x.IsInternInBatchAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inBatch);
        m.Setup(x => x.IsMentorAssignedToInternAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        return m;
    }

    /// <summary>Setup project + mentor (Lead) + intern (Member).</summary>
    private static async Task<(Guid InternshipId, Guid MentorId, Guid InternId, Guid ProjectId)>
        SetupProjectWithIntern(InternFlowDbContext db)
    {
        var internshipId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var mentorId = Guid.NewGuid();
        var internId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        db.Internships.Add(EntityFactory.CreateInternship(internshipId, adminId));
        db.Users.AddRange(
            EntityFactory.CreateUser(mentorId, UserRole.Mentor),
            EntityFactory.CreateUser(internId, UserRole.Intern));

        var project = EntityFactory.CreateProject(internshipId, mentorId);
        project.Id = projectId;
        db.Projects.Add(project);

        db.ProjectMembers.AddRange(
            new ProjectMember
            {
                ProjectId = projectId,
                UserId = mentorId,
                Role = MemberRole.Lead,
                JoinedAt = DateTime.UtcNow
            },
            new ProjectMember
            {
                ProjectId = projectId,
                UserId = internId,
                Role = MemberRole.Member,
                JoinedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync();
        return (internshipId, mentorId, internId, projectId);
    }

    /// <summary>Tạo task trực tiếp trong DB — dùng cho test #T12, #T13, #T14.</summary>
    private static async Task<Guid> SeedTaskAsync(
        InternFlowDbContext db,
        Guid projectId,
        Guid assigneeId,
        Guid createdBy,
        TaskStatus status = TaskStatus.Todo,
        TaskPriority priority = TaskPriority.Medium,
        string title = "Test Task",
        DateOnly? dueDate = null)
    {
        var taskId = Guid.NewGuid();
        db.Tasks.Add(new TaskItem
        {
            Id = taskId,
            ProjectId = projectId,
            AssigneeId = assigneeId,
            CreatedBy = createdBy,
            Title = title,
            Description = "Description",
            Status = status,
            Priority = priority,
            DueDate = dueDate,
            CompletedAt = status == TaskStatus.Done ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return taskId;
    }

    #endregion
    #region Create Task

    [Fact]
    public async Task Create_LeadRole_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            MockInternshipQuery().Object,
            TestMapperFactory.Instance,
            mediatorMock.Object);

        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var request = new CreateTaskRequest(
            "Fix login bug", "Description",
            TaskPriority.High, dueDate, internId);

        // Act
        var result = await handler.Handle(
            new CreateTaskCommand(projectId, request), CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.ProjectId.Should().Be(projectId);
        result.Title.Should().Be("Fix login bug");
        result.Status.Should().Be(nameof(TaskStatus.Todo));
        result.Priority.Should().Be(nameof(TaskPriority.High));
        result.AssigneeId.Should().Be(internId);
        result.DueDate.Should().Be(dueDate);
        result.CreatedBy.Should().Be(mentorId);
        result.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Create_AdminRole_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId) = await SetupProjectWithIntern(db);
        var adminId = Guid.NewGuid();
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            MockInternshipQuery().Object,
            TestMapperFactory.Instance,
            mediatorMock.Object);

        var request = new CreateTaskRequest(
            "Task", null, TaskPriority.Medium, null, internId);

        // Act
        var result = await handler.Handle(
            new CreateTaskCommand(projectId, request), CancellationToken.None);

        // Assert
        result.CreatedBy.Should().Be(adminId);
    }

    [Fact]
    public async Task Create_ProjectNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var adminId = Guid.NewGuid();
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            MockInternshipQuery().Object,
            TestMapperFactory.Instance,
            mediatorMock.Object);

        var request = new CreateTaskRequest(
            "Task", null, TaskPriority.Medium, null, Guid.NewGuid());

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new CreateTaskCommand(Guid.NewGuid(), request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_InternRole_ThrowsForbidden()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId) = await SetupProjectWithIntern(db);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            MockInternshipQuery().Object,
            TestMapperFactory.Instance,
            mediatorMock.Object);

        var request = new CreateTaskRequest(
            "Task", null, TaskPriority.Medium, null, internId);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new CreateTaskCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_NonLeadMentor_ThrowsForbidden()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId) = await SetupProjectWithIntern(db);
        var otherMentorId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(otherMentorId, UserRole.Mentor));
        await db.SaveChangesAsync();
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(otherMentorId, UserRole.Mentor),
            MockInternshipQuery().Object,
            TestMapperFactory.Instance,
            mediatorMock.Object);

        var request = new CreateTaskRequest(
            "Task", null, TaskPriority.Medium, null, internId);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new CreateTaskCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_AssigneeNotIntern_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, _, projectId) = await SetupProjectWithIntern(db);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            MockInternshipQuery().Object,
            TestMapperFactory.Instance,
            mediatorMock.Object);

        // Assignee là Mentor — không phải Intern
        var request = new CreateTaskRequest(
            "Task", null, TaskPriority.Medium, null, mentorId);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateTaskCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_AssigneeNotMember_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, _, projectId) = await SetupProjectWithIntern(db);
        var outsiderId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(outsiderId, UserRole.Intern));
        await db.SaveChangesAsync();
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            MockInternshipQuery().Object,
            TestMapperFactory.Instance,
            mediatorMock.Object);

        var request = new CreateTaskRequest(
            "Task", null, TaskPriority.Medium, null, outsiderId);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateTaskCommand(projectId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Create_InternNotInBatch_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new CreateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            MockInternshipQuery(inBatch: false).Object,
            TestMapperFactory.Instance,
            mediatorMock.Object);

        var request = new CreateTaskRequest(
            "Task", null, TaskPriority.Medium, null, internId);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new CreateTaskCommand(projectId, request),
                CancellationToken.None));
    }

    #endregion

    #region Change Task Status

    [Fact]
    public async Task ChangeStatus_Intern_TodoToInProgress_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId) = await SetupProjectWithIntern(db);
        var taskId = await SeedTaskAsync(db, projectId, internId, internId, TaskStatus.Todo);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        // Act
        var result = await handler.Handle(
            new ChangeTaskStatusCommand(taskId, TaskStatus.InProgress),
            CancellationToken.None);

        // Assert
        result.Status.Should().Be(nameof(TaskStatus.InProgress));
    }

    [Fact]
    public async Task ChangeStatus_Intern_InProgressToInReview_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId) = await SetupProjectWithIntern(db);
        var taskId = await SeedTaskAsync(db, projectId, internId, internId, TaskStatus.InProgress);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        // Act
        var result = await handler.Handle(
            new ChangeTaskStatusCommand(taskId, TaskStatus.InReview),
            CancellationToken.None);

        // Assert
        result.Status.Should().Be(nameof(TaskStatus.InReview));
    }

    [Fact]
    public async Task ChangeStatus_Intern_InReviewToDone_ThrowsForbidden()
    {
        // Arrange — Intern KHÔNG được tự duyệt
        await using var db = TestDbContextFactory.Create();
        var (_, _, internId, projectId) = await SetupProjectWithIntern(db);
        var taskId = await SeedTaskAsync(db, projectId, internId, internId, TaskStatus.InReview);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(
                new ChangeTaskStatusCommand(taskId, TaskStatus.Done),
                CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatus_Mentor_InReviewToDone_Success_SetCompletedAt()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);
        var taskId = await SeedTaskAsync(db, projectId, internId, mentorId, TaskStatus.InReview);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        // Act
        var result = await handler.Handle(
            new ChangeTaskStatusCommand(taskId, TaskStatus.Done),
            CancellationToken.None);

        // Assert
        result.Status.Should().Be(nameof(TaskStatus.Done));
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ChangeStatus_Mentor_InReviewToInProgress_Success()
    {
        // Arrange — Mentor trả task lại
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);
        var taskId = await SeedTaskAsync(db, projectId, internId, mentorId, TaskStatus.InReview);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        // Act
        var result = await handler.Handle(
            new ChangeTaskStatusCommand(taskId, TaskStatus.InProgress),
            CancellationToken.None);

        // Assert
        result.Status.Should().Be(nameof(TaskStatus.InProgress));
    }

    [Fact]
    public async Task ChangeStatus_SkipStep_ThrowsConflict()
    {
        // Arrange — Todo → Done (bỏ qua bước)
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);
        var taskId = await SeedTaskAsync(db, projectId, internId, mentorId, TaskStatus.Todo);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new ChangeTaskStatusCommand(taskId, TaskStatus.Done),
                CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatus_TerminalState_ThrowsConflict()
    {
        // Arrange — Done → Todo (terminal)
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);
        var taskId = await SeedTaskAsync(db, projectId, internId, mentorId, TaskStatus.Done);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(
                new ChangeTaskStatusCommand(taskId, TaskStatus.Todo),
                CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatus_Intern_OtherTask_ThrowsNotFound()
    {
        // Arrange — Intern thao tác task không phải của mình
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);
        var otherInternId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(otherInternId, UserRole.Intern));
        await db.SaveChangesAsync();

        var taskId = await SeedTaskAsync(db, projectId, otherInternId, mentorId, TaskStatus.Todo);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new ChangeTaskStatusCommand(taskId, TaskStatus.InProgress),
                CancellationToken.None));
    }

    [Fact]
    public async Task ChangeStatus_TaskNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var adminId = Guid.NewGuid();
        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new ChangeTaskStatusCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new ChangeTaskStatusCommand(Guid.NewGuid(), TaskStatus.InProgress),
                CancellationToken.None));
    }

    #endregion

    #region Query Tasks

    [Fact]
    public async Task GetTasks_Admin_SeesAllTasks()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);

        await SeedTaskAsync(db, projectId, internId, mentorId, TaskStatus.Todo, title: "T1");
        await SeedTaskAsync(db, projectId, internId, mentorId, TaskStatus.InProgress, title: "T2");
        await SeedTaskAsync(db, projectId, internId, mentorId, TaskStatus.Done, title: "T3");

        var handler = new GetTasksQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin));

        // Act
        var result = await handler.Handle(
            new GetTasksQuery(new TaskQueryParameters()), CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetTasks_Intern_SeesOnlyAssignedTasks()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);
        var otherInternId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(otherInternId, UserRole.Intern));
        await db.SaveChangesAsync();

        await SeedTaskAsync(db, projectId, internId, mentorId, title: "My task");
        await SeedTaskAsync(db, projectId, otherInternId, mentorId, title: "Other task");

        var handler = new GetTasksQueryHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern));

        // Act
        var result = await handler.Handle(
            new GetTasksQuery(new TaskQueryParameters()), CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].Title.Should().Be("My task");
    }

    [Fact]
    public async Task GetTasks_FilterByStatus_ReturnsCorrect()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);

        await SeedTaskAsync(db, projectId, internId, mentorId, TaskStatus.Todo);
        await SeedTaskAsync(db, projectId, internId, mentorId, TaskStatus.InProgress);
        await SeedTaskAsync(db, projectId, internId, mentorId, TaskStatus.InProgress);

        var handler = new GetTasksQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin));

        // Act
        var result = await handler.Handle(
            new GetTasksQuery(new TaskQueryParameters
            {
                Status = TaskStatus.InProgress
            }), CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetTasks_SortByDueDateAsc_ReturnsCorrect()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        await SeedTaskAsync(db, projectId, internId, mentorId,
            title: "Later", dueDate: today.AddDays(10));
        await SeedTaskAsync(db, projectId, internId, mentorId,
            title: "Sooner", dueDate: today.AddDays(1));

        var handler = new GetTasksQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin));

        // Act
        var result = await handler.Handle(
            new GetTasksQuery(new TaskQueryParameters
            {
                SortBy = TaskSortFields.DueDate,
                SortDir = "asc"
            }), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Select(x => x.Title).Should().BeEquivalentTo(new[] { "Later", "Sooner" });
    }

    [Fact]
    public async Task GetTasks_PageSizeOver50_ClampsTo50()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();

        var handler = new GetTasksQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin));

        // Act
        var result = await handler.Handle(
            new GetTasksQuery(new TaskQueryParameters
            {
                PageSize = 1000
            }), CancellationToken.None);

        // Assert
        result.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task GetTasks_FilterByProjectId_ReturnsCorrect()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);

        // Project khác
        var otherInternshipId = Guid.NewGuid();
        var otherProjectId = Guid.NewGuid();
        db.Internships.Add(EntityFactory.CreateInternship(otherInternshipId, mentorId));
        var otherProject = EntityFactory.CreateProject(otherInternshipId, mentorId);
        otherProject.Id = otherProjectId;
        db.Projects.Add(otherProject);
        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = otherProjectId,
            UserId = internId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        await SeedTaskAsync(db, projectId, internId, mentorId, title: "P1 Task");
        await SeedTaskAsync(db, otherProjectId, internId, mentorId, title: "P2 Task");

        var handler = new GetTasksQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin));

        // Act
        var result = await handler.Handle(
            new GetTasksQuery(new TaskQueryParameters { ProjectId = projectId }),
            CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items[0].Title.Should().Be("P1 Task");
    }

    [Fact]
    public async Task GetTasks_KeywordFilter_ReturnsMatching()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);

        await SeedTaskAsync(db, projectId, internId, mentorId, title: "Fix login bug");
        await SeedTaskAsync(db, projectId, internId, mentorId, title: "Fix logout bug");
        await SeedTaskAsync(db, projectId, internId, mentorId, title: "Add feature");

        var handler = new GetTasksQueryHandler(
            db,
            MockCurrentUserFactory.Create(Guid.NewGuid(), UserRole.Admin));

        // Act
        var result = await handler.Handle(
            new GetTasksQuery(new TaskQueryParameters { Keyword = "Fix" }),
            CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    #endregion

    #region Update Task

    [Fact]
    public async Task Update_Mentor_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);
        var taskId = await SeedTaskAsync(db, projectId, internId, mentorId);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new UpdateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        var request = new UpdateTaskRequest(
            "Updated Title", "Updated Desc",
            TaskPriority.High, null, internId);

        // Act
        var result = await handler.Handle(
            new UpdateTaskCommand(taskId, request), CancellationToken.None);

        // Assert
        result.Title.Should().Be("Updated Title");
        result.Description.Should().Be("Updated Desc");
        result.Priority.Should().Be(nameof(TaskPriority.High));
    }

    [Fact]
    public async Task Update_ChangeAssignee_ValidIntern_Success()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);
        var newInternId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(newInternId, UserRole.Intern));
        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = projectId,
            UserId = newInternId,
            Role = MemberRole.Member,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var taskId = await SeedTaskAsync(db, projectId, internId, mentorId);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new UpdateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        var request = new UpdateTaskRequest(
            "Task", null, TaskPriority.Medium, null, newInternId);

        // Act
        var result = await handler.Handle(
            new UpdateTaskCommand(taskId, request), CancellationToken.None);

        // Assert
        result.AssigneeId.Should().Be(newInternId);
    }

    [Fact]
    public async Task Update_ChangeAssignee_NotIntern_ThrowsConflict()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);
        var taskId = await SeedTaskAsync(db, projectId, internId, mentorId);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new UpdateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(mentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        // Assignee mới là Mentor — không phải Intern
        var request = new UpdateTaskRequest(
            "Task", null, TaskPriority.Medium, null, mentorId);

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.Handle(new UpdateTaskCommand(taskId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Update_Intern_ThrowsForbidden()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);
        var taskId = await SeedTaskAsync(db, projectId, internId, mentorId);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new UpdateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(internId, UserRole.Intern),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        var request = new UpdateTaskRequest(
            "Updated", null, TaskPriority.Medium, null, internId);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new UpdateTaskCommand(taskId, request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Update_TaskNotFound_ThrowsNotFound()
    {
        // Arrange
        await using var db = TestDbContextFactory.Create();
        var adminId = Guid.NewGuid();
        var mediatorMock = MockMediatorFactory.CreateNoOp();
        var handler = new UpdateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(adminId, UserRole.Admin),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        var request = new UpdateTaskRequest(
            "Task", null, TaskPriority.Medium, null, Guid.NewGuid());

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new UpdateTaskCommand(Guid.NewGuid(), request),
                CancellationToken.None));
    }

    [Fact]
    public async Task Update_NonMemberMentor_ThrowsNotFound()
    {
        // Arrange — Mentor không thuộc project
        await using var db = TestDbContextFactory.Create();
        var (_, mentorId, internId, projectId) = await SetupProjectWithIntern(db);
        var otherMentorId = Guid.NewGuid();

        db.Users.Add(EntityFactory.CreateUser(otherMentorId, UserRole.Mentor));
        await db.SaveChangesAsync();

        var taskId = await SeedTaskAsync(db, projectId, internId, mentorId);
        var mediatorMock = MockMediatorFactory.CreateNoOp();

        var handler = new UpdateTaskCommandHandler(
            db,
            MockCurrentUserFactory.Create(otherMentorId, UserRole.Mentor),
            TestMapperFactory.Instance,
            mediatorMock.Object);

        var request = new UpdateTaskRequest(
            "Task", null, TaskPriority.Medium, null, internId);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new UpdateTaskCommand(taskId, request),
                CancellationToken.None));
    }

    #endregion
}

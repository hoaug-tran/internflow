using InternFlow.Application.Features.MentorAssignments.Commands.AssignMentor;
using InternFlow.Domain.Identity;
using InternFlow.Domain.Internships;
using InternFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InternFlow.Tests.Internships;

public class AssignMentorTests : IDisposable
{
    private readonly InternFlowDbContext _context;
    private readonly AssignMentorCommandHandler _handler;

    public AssignMentorTests()
    {
        var options = new DbContextOptionsBuilder<InternFlowDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new InternFlowDbContext(options);
        _handler = new AssignMentorCommandHandler(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<(Internship batch, User mentor, User intern)> SeedBatchWithMentorAndInternAsync(int? maxMentees = 5)
    {
        var batch = new Internship
        {
            Id = Guid.NewGuid(),
            Name = "Batch Summer 2026",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)),
            Status = InternshipStatus.Active
        };

        var mentor = new User
        {
            Id = Guid.NewGuid(),
            Email = "mentor@internflow.com",
            FullName = "Mentor Test",
            PasswordHash = "hash",
            Role = UserRole.Mentor,
            IsActive = true
        };

        var mentorProfile = new MentorProfile
        {
            UserId = mentor.Id,
            Department = "Engineering",
            MaxMentees = maxMentees
        };

        var intern = new User
        {
            Id = Guid.NewGuid(),
            Email = "intern@internflow.com",
            FullName = "Intern Test",
            PasswordHash = "hash",
            Role = UserRole.Intern,
            IsActive = true
        };

        var internProfile = new InternProfile
        {
            UserId = intern.Id,
            University = "FPT University"
        };

        var batchMentor = new BatchMentor
        {
            InternshipId = batch.Id,
            MentorId = mentor.Id,
            JoinedAt = DateTime.UtcNow
        };

        var batchIntern = new BatchIntern
        {
            InternshipId = batch.Id,
            InternId = intern.Id,
            Status = BatchInternStatus.InProgress,
            JoinedAt = DateTime.UtcNow
        };

        _context.Internships.Add(batch);
        _context.Users.AddRange(mentor, intern);
        _context.MentorProfiles.Add(mentorProfile);
        _context.InternProfiles.Add(internProfile);
        _context.BatchMentors.Add(batchMentor);
        _context.BatchInterns.Add(batchIntern);
        await _context.SaveChangesAsync();

        return (batch, mentor, intern);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldAssignMentorSuccessfully()
    {
        // Arrange
        var (batch, mentor, intern) = await SeedBatchWithMentorAndInternAsync();
        var command = new AssignMentorCommand(batch.Id, intern.Id, mentor.Id, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data > 0);

        var assignment = await _context.MentorAssignments.FirstOrDefaultAsync(ma => ma.Id == result.Data);
        Assert.NotNull(assignment);
        Assert.Equal(mentor.Id, assignment.MentorId);
        Assert.Equal(intern.Id, assignment.InternId);
        Assert.Null(assignment.EndedAt);
    }

    [Fact]
    public async Task Handle_WhenMentorReachesMaxMenteesLimit_ShouldFail()
    {
        // Arrange - Mentor allows maximum 1 mentee
        var (batch, mentor, intern1) = await SeedBatchWithMentorAndInternAsync(maxMentees: 1);

        // Mentor already has 1 active assignment
        _context.MentorAssignments.Add(new MentorAssignment
        {
            InternshipId = batch.Id,
            MentorId = mentor.Id,
            InternId = intern1.Id,
            AssignedAt = DateTime.UtcNow
        });

        // Create a 2nd intern in the same batch
        var intern2 = new User
        {
            Id = Guid.NewGuid(),
            Email = "intern2@internflow.com",
            FullName = "Intern 2",
            PasswordHash = "hash",
            Role = UserRole.Intern,
            IsActive = true
        };
        _context.Users.Add(intern2);
        _context.BatchInterns.Add(new BatchIntern
        {
            InternshipId = batch.Id,
            InternId = intern2.Id,
            Status = BatchInternStatus.InProgress,
            JoinedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act - Try to assign 2nd intern to mentor
        var command = new AssignMentorCommand(batch.Id, intern2.Id, mentor.Id, Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("giới hạn tối đa", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_WhenInternReassignedToNewMentor_ShouldEndPreviousAssignmentAutomatically()
    {
        // Arrange
        var (batch, mentorA, intern) = await SeedBatchWithMentorAndInternAsync();

        // Intern currently assigned to Mentor A
        var oldAssignment = new MentorAssignment
        {
            InternshipId = batch.Id,
            MentorId = mentorA.Id,
            InternId = intern.Id,
            AssignedAt = DateTime.UtcNow.AddDays(-5)
        };
        _context.MentorAssignments.Add(oldAssignment);

        // Create Mentor B in same batch
        var mentorB = new User
        {
            Id = Guid.NewGuid(),
            Email = "mentorB@internflow.com",
            FullName = "Mentor B",
            PasswordHash = "hash",
            Role = UserRole.Mentor,
            IsActive = true
        };
        _context.Users.Add(mentorB);
        _context.MentorProfiles.Add(new MentorProfile { UserId = mentorB.Id, MaxMentees = 5 });
        _context.BatchMentors.Add(new BatchMentor { InternshipId = batch.Id, MentorId = mentorB.Id, JoinedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        // Act - Reassign intern to Mentor B
        var command = new AssignMentorCommand(batch.Id, intern.Id, mentorB.Id, Guid.NewGuid());
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        // Check old assignment is ended
        var oldInDb = await _context.MentorAssignments.FirstAsync(ma => ma.Id == oldAssignment.Id);
        Assert.NotNull(oldInDb.EndedAt);
        Assert.Contains("Reassigned", oldInDb.EndReason);

        // Check new assignment is active
        var newAssignment = await _context.MentorAssignments.FirstAsync(ma => ma.Id == result.Data);
        Assert.Equal(mentorB.Id, newAssignment.MentorId);
        Assert.Null(newAssignment.EndedAt);
    }

    [Fact]
    public async Task Handle_WhenInternNotInBatch_ShouldFail()
    {
        // Arrange
        var (batch, mentor, _) = await SeedBatchWithMentorAndInternAsync();
        var strangerInternId = Guid.NewGuid();

        var command = new AssignMentorCommand(batch.Id, strangerInternId, mentor.Id, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("chưa được thêm vào đợt", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}

using FluentAssertions;
using InternFlow.Application.Features.MentorAssignments.Commands.AssignMentor;
using InternFlow.Application.Features.MentorAssignments.Commands.EndMentorAssignment;
using InternFlow.Application.Features.MentorAssignments.Queries.GetMentorAssignments;
using Xunit;

namespace InternFlow.Tests.MentorAssignments;

public class MentorAssignmentValidationTests
{
    private readonly AssignMentorCommandValidator _assignValidator = new();
    private readonly EndMentorAssignmentCommandValidator _endValidator = new();
    private readonly GetMentorAssignmentsQueryValidator _getQueryValidator = new();

    #region AssignMentorCommandValidator Tests

    [Fact]
    public void AssignMentor_ValidCommand_ShouldPassValidation()
    {
        var command = new AssignMentorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        var result = _assignValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AssignMentor_WithoutExplicitAssignedBy_ShouldPassValidation()
    {
        var command = new AssignMentorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        var result = _assignValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void AssignMentor_EmptyInternshipId_ShouldFailValidation()
    {
        var command = new AssignMentorCommand(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid());

        var result = _assignValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "InternshipId");
    }

    [Fact]
    public void AssignMentor_EmptyInternId_ShouldFailValidation()
    {
        var command = new AssignMentorCommand(
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid());

        var result = _assignValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "InternId");
    }

    [Fact]
    public void AssignMentor_EmptyMentorId_ShouldFailValidation()
    {
        var command = new AssignMentorCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid());

        var result = _assignValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MentorId");
    }

    #endregion

    #region EndMentorAssignmentCommandValidator Tests

    [Fact]
    public void EndMentorAssignment_ValidCommand_ShouldPassValidation()
    {
        var command = new EndMentorAssignmentCommand(123, "Hoàn thành kỳ thực tập");

        var result = _endValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void EndMentorAssignment_InvalidAssignmentId_ShouldFailValidation(long assignmentId)
    {
        var command = new EndMentorAssignmentCommand(assignmentId);

        var result = _endValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AssignmentId");
    }

    [Fact]
    public void EndMentorAssignment_ReasonTooLong_ShouldFailValidation()
    {
        var command = new EndMentorAssignmentCommand(1, new string('a', 501));

        var result = _endValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EndReason");
    }

    #endregion

    #region GetMentorAssignmentsQueryValidator Tests

    [Fact]
    public void GetMentorAssignments_ValidQuery_ShouldPassValidation()
    {
        var query = new GetMentorAssignmentsQuery(Guid.NewGuid());

        var result = _getQueryValidator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetMentorAssignments_EmptyInternshipId_ShouldFailValidation()
    {
        var query = new GetMentorAssignmentsQuery(Guid.Empty);

        var result = _getQueryValidator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "InternshipId");
    }

    #endregion
}

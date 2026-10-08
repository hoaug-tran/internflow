using FluentAssertions;
using InternFlow.Application.Features.Internships.Commands.AddBatchIntern;
using InternFlow.Application.Features.Internships.Commands.AddBatchMentor;
using InternFlow.Application.Features.Internships.Commands.CreateInternship;
using InternFlow.Application.Features.Internships.Commands.UpdateBatchInternStatus;
using InternFlow.Application.Features.Internships.Commands.UpdateInternshipStatus;
using InternFlow.Application.Features.Internships.Queries.GetInternshipById;
using InternFlow.Application.Features.Internships.Queries.GetInternships;
using InternFlow.Domain.Internships;
using Xunit;

namespace InternFlow.Tests.Internships;

public class InternshipValidationTests
{
    private readonly CreateInternshipCommandValidator _createValidator = new();
    private readonly UpdateInternshipStatusCommandValidator _updateStatusValidator = new();
    private readonly AddBatchMentorCommandValidator _addMentorValidator = new();
    private readonly AddBatchInternCommandValidator _addInternValidator = new();
    private readonly UpdateBatchInternStatusCommandValidator _updateInternStatusValidator = new();
    private readonly GetInternshipsQueryValidator _getInternshipsValidator = new();
    private readonly GetInternshipByIdQueryValidator _getByIdValidator = new();

    #region CreateInternshipCommandValidator Tests

    [Fact]
    public void CreateInternship_ValidCommand_ShouldPassValidation()
    {
        var command = new CreateInternshipCommand(
            "Batch Autumn 2026",
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 12, 31),
            Guid.NewGuid());

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateInternship_WithoutExplicitCreatedBy_ShouldPassValidation()
    {
        var command = new CreateInternshipCommand(
            "Batch Autumn 2026",
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 12, 31));

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void CreateInternship_EmptyName_ShouldFailValidation(string name)
    {
        var command = new CreateInternshipCommand(
            name,
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 12, 31),
            Guid.NewGuid());

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateInternship_NameTooLong_ShouldFailValidation()
    {
        var longName = new string('A', 201);
        var command = new CreateInternshipCommand(
            longName,
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 12, 31),
            Guid.NewGuid());

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateInternship_EndDateBeforeStartDate_ShouldFailValidation()
    {
        var command = new CreateInternshipCommand(
            "Batch Autumn 2026",
            new DateOnly(2026, 12, 31),
            new DateOnly(2026, 9, 1),
            Guid.NewGuid());

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EndDate");
    }

    [Fact]
    public void CreateInternship_EmptyCreatedBy_ShouldFailValidation()
    {
        var command = new CreateInternshipCommand(
            "Batch Autumn 2026",
            new DateOnly(2026, 9, 1),
            new DateOnly(2026, 12, 31),
            Guid.Empty);

        var result = _createValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CreatedBy");
    }

    #endregion

    #region UpdateInternshipStatusCommandValidator Tests

    [Fact]
    public void UpdateStatus_ValidCommand_ShouldPassValidation()
    {
        var command = new UpdateInternshipStatusCommand(Guid.NewGuid(), InternshipStatus.Active);

        var result = _updateStatusValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateStatus_EmptyInternshipId_ShouldFailValidation()
    {
        var command = new UpdateInternshipStatusCommand(Guid.Empty, InternshipStatus.Active);

        var result = _updateStatusValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "InternshipId");
    }

    [Fact]
    public void UpdateStatus_InvalidStatusEnum_ShouldFailValidation()
    {
        var command = new UpdateInternshipStatusCommand(Guid.NewGuid(), (InternshipStatus)999);

        var result = _updateStatusValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewStatus");
    }

    #endregion

    #region Batch Mentor & Intern Tests

    [Fact]
    public void AddBatchMentor_EmptyMentorId_ShouldFailValidation()
    {
        var command = new AddBatchMentorCommand(Guid.NewGuid(), Guid.Empty, Guid.NewGuid());

        var result = _addMentorValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MentorId");
    }

    [Fact]
    public void AddBatchIntern_NotesTooLong_ShouldFailValidation()
    {
        var command = new AddBatchInternCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new string('a', 501));

        var result = _addInternValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Notes");
    }

    [Fact]
    public void UpdateBatchInternStatus_InvalidEnum_ShouldFailValidation()
    {
        var command = new UpdateBatchInternStatusCommand(Guid.NewGuid(), Guid.NewGuid(), (BatchInternStatus)999);

        var result = _updateInternStatusValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewStatus");
    }

    #endregion

    #region Query Validators Tests

    [Fact]
    public void GetInternships_ValidQuery_ShouldPassValidation()
    {
        var query = new GetInternshipsQuery(Page: 1, PageSize: 20);

        var result = _getInternshipsValidator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetInternships_InvalidPage_ShouldFailValidation(int page)
    {
        var query = new GetInternshipsQuery(Page: page, PageSize: 10);

        var result = _getInternshipsValidator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Page");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void GetInternships_InvalidPageSize_ShouldFailValidation(int pageSize)
    {
        var query = new GetInternshipsQuery(Page: 1, PageSize: pageSize);

        var result = _getInternshipsValidator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void GetInternshipById_EmptyId_ShouldFailValidation()
    {
        var query = new GetInternshipByIdQuery(Guid.Empty);

        var result = _getByIdValidator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    #endregion
}

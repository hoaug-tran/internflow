using FluentValidation;

namespace InternFlow.Application.Features.Internships.Commands.AddBatchMentor;

public sealed class AddBatchMentorCommandValidator : AbstractValidator<AddBatchMentorCommand>
{
    public AddBatchMentorCommandValidator()
    {
        RuleFor(x => x.InternshipId)
            .NotEmpty().WithMessage("InternshipId không được để trống.");

        RuleFor(x => x.MentorId)
            .NotEmpty().WithMessage("MentorId không được để trống.");

        RuleFor(x => x.AddedBy)
            .NotEqual(Guid.Empty).WithMessage("Người thêm không được để trống.")
            .When(x => x.AddedBy.HasValue);
    }
}

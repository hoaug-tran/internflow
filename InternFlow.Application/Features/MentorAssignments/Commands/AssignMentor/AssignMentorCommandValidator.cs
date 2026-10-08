using FluentValidation;

namespace InternFlow.Application.Features.MentorAssignments.Commands.AssignMentor;

public sealed class AssignMentorCommandValidator : AbstractValidator<AssignMentorCommand>
{
    public AssignMentorCommandValidator()
    {
        RuleFor(x => x.InternshipId)
            .NotEmpty().WithMessage("InternshipId không được để trống.");

        RuleFor(x => x.InternId)
            .NotEmpty().WithMessage("InternId không được để trống.");

        RuleFor(x => x.MentorId)
            .NotEmpty().WithMessage("MentorId không được để trống.");

        RuleFor(x => x.AssignedBy)
            .NotEqual(Guid.Empty).WithMessage("Người phân công không được để trống.")
            .When(x => x.AssignedBy.HasValue);
    }
}

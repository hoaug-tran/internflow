using FluentValidation;

namespace InternFlow.Application.Features.MentorAssignments.Commands.EndMentorAssignment;

public sealed class EndMentorAssignmentCommandValidator : AbstractValidator<EndMentorAssignmentCommand>
{
    public EndMentorAssignmentCommandValidator()
    {
        RuleFor(x => x.AssignmentId)
            .GreaterThan(0).WithMessage("AssignmentId không hợp lệ.");

        RuleFor(x => x.EndReason)
            .MaximumLength(500).WithMessage("Lý do kết thúc không được vượt quá 500 ký tự.")
            .When(x => !string.IsNullOrEmpty(x.EndReason));
    }
}

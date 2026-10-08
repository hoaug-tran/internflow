using FluentValidation;

namespace InternFlow.Application.Features.MentorAssignments.Queries.GetMentorAssignments;

public sealed class GetMentorAssignmentsQueryValidator : AbstractValidator<GetMentorAssignmentsQuery>
{
    public GetMentorAssignmentsQueryValidator()
    {
        RuleFor(x => x.InternshipId)
            .NotEmpty().WithMessage("InternshipId không được để trống.");
    }
}

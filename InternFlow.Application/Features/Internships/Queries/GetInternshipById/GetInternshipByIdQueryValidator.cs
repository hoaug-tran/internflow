using FluentValidation;

namespace InternFlow.Application.Features.Internships.Queries.GetInternshipById;

public sealed class GetInternshipByIdQueryValidator : AbstractValidator<GetInternshipByIdQuery>
{
    public GetInternshipByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id đợt thực tập không được để trống.");
    }
}

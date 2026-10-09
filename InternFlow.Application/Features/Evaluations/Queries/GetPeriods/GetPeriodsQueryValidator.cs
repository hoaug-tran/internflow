using FluentValidation;

namespace InternFlow.Application.Features.Evaluations.Queries.GetPeriods;

public sealed class GetPeriodsQueryValidator : AbstractValidator<GetPeriodsQuery>
{
    public GetPeriodsQueryValidator()
    {
        RuleFor(x => x.InternshipId)
            .NotEmpty().When(x => x.InternshipId.HasValue)
            .WithMessage("InternshipId không hợp lệ.");
    }
}

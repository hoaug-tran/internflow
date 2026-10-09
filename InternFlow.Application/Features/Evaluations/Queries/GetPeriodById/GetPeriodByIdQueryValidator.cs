using FluentValidation;

namespace InternFlow.Application.Features.Evaluations.Queries.GetPeriodById;

public sealed class GetPeriodByIdQueryValidator : AbstractValidator<GetPeriodByIdQuery>
{
    public GetPeriodByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

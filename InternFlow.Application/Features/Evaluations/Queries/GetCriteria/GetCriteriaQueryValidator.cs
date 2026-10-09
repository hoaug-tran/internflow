using FluentValidation;

namespace InternFlow.Application.Features.Evaluations.Queries.GetCriteria;

public sealed class GetCriteriaQueryValidator : AbstractValidator<GetCriteriaQuery>
{
    public GetCriteriaQueryValidator()
    {
        RuleFor(x => x.PeriodId).NotEmpty();
    }
}

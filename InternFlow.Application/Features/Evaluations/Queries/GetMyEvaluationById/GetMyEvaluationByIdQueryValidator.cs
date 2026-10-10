using FluentValidation;

namespace InternFlow.Application.Features.Evaluations.Queries.GetMyEvaluationById;

public sealed class GetMyEvaluationByIdQueryValidator : AbstractValidator<GetMyEvaluationByIdQuery>
{
    public GetMyEvaluationByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

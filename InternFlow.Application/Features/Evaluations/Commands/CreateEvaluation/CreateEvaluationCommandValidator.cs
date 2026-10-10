using FluentValidation;

namespace InternFlow.Application.Features.Evaluations.Commands.CreateEvaluation;

public sealed class CreateEvaluationCommandValidator : AbstractValidator<CreateEvaluationCommand>
{
    public CreateEvaluationCommandValidator()
    {
        RuleFor(x => x.PeriodId).NotEmpty();
        RuleFor(x => x.InternId).NotEmpty();

        RuleFor(x => x.OverallComment).MaximumLength(4000);

        RuleFor(x => x.Scores).NotNull().NotEmpty()
            .WithMessage("Phải có ít nhất một điểm tiêu chí.");

        RuleForEach(x => x.Scores).ChildRules(s =>
        {
            s.RuleFor(i => i.CriterionId).NotEmpty();
            s.RuleFor(i => i.Comment).MaximumLength(2000);
        });
    }
}

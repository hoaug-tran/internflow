using FluentValidation;

namespace InternFlow.Application.Features.Evaluations.Commands.UpdateEvaluation;

public sealed class UpdateEvaluationCommandValidator : AbstractValidator<UpdateEvaluationCommand>
{
    public UpdateEvaluationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

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

using FluentValidation;

namespace InternFlow.Application.Features.Evaluations.Commands.CreateCriterion;

public sealed class CreateCriterionCommandValidator : AbstractValidator<CreateCriterionCommand>
{
    public CreateCriterionCommandValidator()
    {
        RuleFor(x => x.PeriodId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên tiêu chí không được để trống.")
            .MaximumLength(150);

        RuleFor(x => x.Description).MaximumLength(1000);

        RuleFor(x => x.Weight)
            .GreaterThan(0)
            .LessThanOrEqualTo(EvaluationMapping.MaxTotalWeight)
            .WithMessage("Trọng số phải trong khoảng (0, 100].");

        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

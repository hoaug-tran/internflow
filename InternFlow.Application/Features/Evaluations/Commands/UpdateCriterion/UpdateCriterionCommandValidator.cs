using FluentValidation;

namespace InternFlow.Application.Features.Evaluations.Commands.UpdateCriterion;

public sealed class UpdateCriterionCommandValidator : AbstractValidator<UpdateCriterionCommand>
{
    public UpdateCriterionCommandValidator()
    {
        RuleFor(x => x.PeriodId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();

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

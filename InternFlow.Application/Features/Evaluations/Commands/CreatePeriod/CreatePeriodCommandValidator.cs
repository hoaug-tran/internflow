using FluentValidation;

namespace InternFlow.Application.Features.Evaluations.Commands.CreatePeriod;

public sealed class CreatePeriodCommandValidator : AbstractValidator<CreatePeriodCommand>
{
    public CreatePeriodCommandValidator()
    {
        RuleFor(x => x.InternshipId).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên kỳ đánh giá không được để trống.")
            .MaximumLength(150);

        RuleFor(x => x.Description).MaximumLength(2000);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");
    }
}

using FluentValidation;

namespace InternFlow.Application.Features.WorkLogs.Commands.CreateWorkLog;

public sealed class CreateWorkLogCommandValidator
    : AbstractValidator<CreateWorkLogCommand>
{
    public CreateWorkLogCommandValidator()
    {
        RuleFor(x => x.Request.TaskItemId)
            .NotEmpty().WithMessage("TaskItemId không được rỗng.");

        RuleFor(x => x.Request.WorkDate)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("WorkDate không được ở tương lai.");

        RuleFor(x => x.Request.Hours)
            .GreaterThan(0).WithMessage("Hours phải > 0.")
            .LessThanOrEqualTo(12).WithMessage("Hours tối đa 12 giờ/ngày.");

        RuleFor(x => x.Request.Description)
            .NotEmpty().WithMessage("Description không được rỗng.")
            .MaximumLength(2000);
    }
}

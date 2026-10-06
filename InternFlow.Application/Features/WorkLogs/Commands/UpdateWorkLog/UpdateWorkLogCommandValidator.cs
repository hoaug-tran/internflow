using FluentValidation;

namespace InternFlow.Application.Features.WorkLogs.Commands.UpdateWorkLog;

public sealed class UpdateWorkLogCommandValidator
    : AbstractValidator<UpdateWorkLogCommand>
{
    public UpdateWorkLogCommandValidator()
    {
        RuleFor(x => x.WorkLogId).NotEmpty();

        RuleFor(x => x.Request.WorkDate)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("WorkDate không được ở tương lai.");

        RuleFor(x => x.Request.Hours)
            .GreaterThan(0)
            .LessThanOrEqualTo(12);

        RuleFor(x => x.Request.Description)
            .NotEmpty()
            .MaximumLength(2000);
    }
}

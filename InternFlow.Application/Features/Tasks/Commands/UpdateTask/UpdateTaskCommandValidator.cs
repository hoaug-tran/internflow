using FluentValidation;

namespace InternFlow.Application.Features.Tasks.Commands.UpdateTask;

public sealed class UpdateTaskCommandValidator
    : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();

        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Tiêu đề task không được rỗng.")
            .MaximumLength(250);

        RuleFor(x => x.Request.Description)
            .MaximumLength(4000)
            .When(x => x.Request.Description is not null);

        RuleFor(x => x.Request.AssigneeId)
            .NotEmpty().WithMessage("Phải chỉ định người được giao task.");

        RuleFor(x => x.Request.DueDate)
            .Must(BeTodayOrFuture)
            .WithMessage("DueDate không được ở quá khứ.");
    }

    private static bool BeTodayOrFuture(DateOnly? dueDate)
    {
        if (!dueDate.HasValue) return true;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return dueDate.Value >= today;
    }
}

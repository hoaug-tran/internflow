using FluentValidation;

namespace InternFlow.Application.Features.Tasks.Commands.ChangeTaskStatus;

public sealed class ChangeTaskStatusCommandValidator
    : AbstractValidator<ChangeTaskStatusCommand>
{
    public ChangeTaskStatusCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId không được rỗng.");
    }
}

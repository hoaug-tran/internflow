using FluentValidation;

namespace InternFlow.Application.Features.WorkLogs.Commands.RestoreWorkLog;

public sealed class RestoreWorkLogCommandValidator
    : AbstractValidator<RestoreWorkLogCommand>
{
    public RestoreWorkLogCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

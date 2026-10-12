using FluentValidation;

namespace InternFlow.Application.Features.Projects.Commands.RestoreProjectMember;

public sealed class RestoreProjectMemberCommandValidator
    : AbstractValidator<RestoreProjectMemberCommand>
{
    public RestoreProjectMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

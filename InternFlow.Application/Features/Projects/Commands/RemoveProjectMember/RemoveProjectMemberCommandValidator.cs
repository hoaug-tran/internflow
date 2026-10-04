using FluentValidation;

namespace InternFlow.Application.Features.Projects.Commands.RemoveProjectMember;

public sealed class RemoveProjectMemberCommandValidator
    : AbstractValidator<RemoveProjectMemberCommand>
{
    public RemoveProjectMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

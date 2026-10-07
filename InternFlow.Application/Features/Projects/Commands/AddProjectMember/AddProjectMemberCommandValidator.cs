using FluentValidation;

namespace InternFlow.Application.Features.Projects.Commands.AddProjectMember;

public sealed class AddProjectMemberCommandValidator
    : AbstractValidator<AddProjectMemberCommand>
{
    public AddProjectMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId không được rỗng.");

        RuleFor(x => x.Request.UserId)
            .NotEmpty().WithMessage("UserId không được rỗng.");

        RuleFor(x => x.Request.Role)
            .IsInEnum().WithMessage("Role không hợp lệ.");
    }
}

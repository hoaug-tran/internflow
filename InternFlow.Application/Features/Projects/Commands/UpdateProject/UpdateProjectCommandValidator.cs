using FluentValidation;

namespace InternFlow.Application.Features.Projects.Commands.UpdateProject;

public sealed class UpdateProjectCommandValidator
    : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();

        RuleFor(x => x.Request.Name)
            .NotEmpty().MaximumLength(150);

        RuleFor(x => x.Request.Description)
            .MaximumLength(2000)
            .When(x => x.Request.Description is not null);

        RuleFor(x => x.Request)
            .Must(r => !r.StartDate.HasValue
                    || !r.EndDate.HasValue
                    || r.EndDate >= r.StartDate)
            .WithMessage("EndDate phải sau StartDate.");
    }
}

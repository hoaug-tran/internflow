using FluentValidation;

namespace InternFlow.Application.Features.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Request.InternshipId).NotEmpty();
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.Description).MaximumLength(2000)
            .When(x => x.Request.Description is not null);
        RuleFor(x => x.Request)
            .Must(r => !r.StartDate.HasValue || !r.EndDate.HasValue || r.EndDate >= r.StartDate)
            .WithMessage("Ngày kết thúc phải sau Ngày bắt đầu.");
    }
}

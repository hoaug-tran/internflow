using FluentValidation;

namespace InternFlow.Application.Features.Internships.Commands.CreateInternship;

public sealed class CreateInternshipCommandValidator : AbstractValidator<CreateInternshipCommand>
{
    public CreateInternshipCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên đợt thực tập không được để trống.")
            .MaximumLength(200).WithMessage("Tên đợt thực tập không được vượt quá 200 ký tự.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Ngày bắt đầu không được để trống.");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("Ngày kết thúc không được để trống.")
            .GreaterThan(x => x.StartDate).WithMessage("Ngày kết thúc đợt thực tập phải sau ngày bắt đầu.");

        RuleFor(x => x.CreatedBy)
            .NotEqual(Guid.Empty).WithMessage("Người tạo không được để trống.")
            .When(x => x.CreatedBy.HasValue);
    }
}

using FluentValidation;

namespace InternFlow.Application.Features.Internships.Commands.AddBatchIntern;

public sealed class AddBatchInternCommandValidator : AbstractValidator<AddBatchInternCommand>
{
    public AddBatchInternCommandValidator()
    {
        RuleFor(x => x.InternshipId)
            .NotEmpty().WithMessage("InternshipId không được để trống.");

        RuleFor(x => x.InternId)
            .NotEmpty().WithMessage("InternId không được để trống.");

        RuleFor(x => x.AddedBy)
            .NotEqual(Guid.Empty).WithMessage("Người thêm không được để trống.")
            .When(x => x.AddedBy.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Ghi chú không được vượt quá 500 ký tự.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

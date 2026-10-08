using FluentValidation;

namespace InternFlow.Application.Features.Internships.Commands.UpdateBatchInternStatus;

public sealed class UpdateBatchInternStatusCommandValidator : AbstractValidator<UpdateBatchInternStatusCommand>
{
    public UpdateBatchInternStatusCommandValidator()
    {
        RuleFor(x => x.InternshipId)
            .NotEmpty().WithMessage("InternshipId không được để trống.");

        RuleFor(x => x.InternId)
            .NotEmpty().WithMessage("InternId không được để trống.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Trạng thái thực tập sinh không hợp lệ.");
    }
}

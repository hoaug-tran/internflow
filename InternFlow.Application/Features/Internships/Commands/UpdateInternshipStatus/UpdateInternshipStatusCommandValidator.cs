using FluentValidation;

namespace InternFlow.Application.Features.Internships.Commands.UpdateInternshipStatus;

public sealed class UpdateInternshipStatusCommandValidator : AbstractValidator<UpdateInternshipStatusCommand>
{
    public UpdateInternshipStatusCommandValidator()
    {
        RuleFor(x => x.InternshipId)
            .NotEmpty().WithMessage("InternshipId không được để trống.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Trạng thái đợt thực tập không hợp lệ.");
    }
}

using FluentValidation;

namespace InternFlow.Application.Features.Auth.Commands.RevokeToken;

public sealed class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token không được để trống.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("Lý do thu hồi không được vượt quá 500 ký tự.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}

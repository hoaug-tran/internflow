using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Auth.Commands.RevokeToken;

public record RevokeTokenCommand(
    string RefreshToken,
    string? Reason = null,
    string? IpAddress = null
) : IRequest<ApiResponse<bool>>;

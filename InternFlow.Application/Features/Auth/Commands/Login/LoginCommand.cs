using InternFlow.Application.Features.Auth.DTOs;
using InternFlow.Shared.Common;
using MediatR;

namespace InternFlow.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<ApiResponse<AuthResponse>>;

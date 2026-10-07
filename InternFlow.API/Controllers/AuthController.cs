using InternFlow.API.Controllers.Conventions;
using InternFlow.API.Extensions;
using InternFlow.Application.Features.Auth.Commands.Login;
using InternFlow.Application.Features.Auth.Commands.RefreshToken;
using InternFlow.Application.Features.Auth.Commands.RevokeToken;
using InternFlow.Application.Features.Auth.DTOs;
using InternFlow.Application.Features.Auth.Queries.GetCurrentUser;
using InternFlow.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InternFlow.API.Controllers;

[Route("api/[controller]")]
public class AuthController : ApiControllerBase
{
    [HttpPost("login")]
    public async Task<ApiResponse<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        return await Sender.Send(new LoginCommand(
            request.Email,
            request.Password,
            HttpContext.GetIpAddress(),
            HttpContext.GetUserAgent()
        ), ct);
    }

    [HttpPost("refresh-token")]
    public async Task<ApiResponse<AuthResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken ct)
    {
        return await Sender.Send(new RefreshTokenCommand(
            request.RefreshToken,
            HttpContext.GetIpAddress(),
            HttpContext.GetUserAgent()
        ), ct);
    }

    [HttpPost("revoke-token")]
    public async Task<ApiResponse<bool>> RevokeToken(
        [FromBody] RevokeTokenRequest request,
        CancellationToken ct)
    {
        return await Sender.Send(new RevokeTokenCommand(
            request.RefreshToken,
            request.Reason,
            HttpContext.GetIpAddress()
        ), ct);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ApiResponse<UserInfoResponse>> GetCurrentUser(CancellationToken ct)
    {
        return await Sender.Send(new GetCurrentUserQuery(), ct);
    }
}

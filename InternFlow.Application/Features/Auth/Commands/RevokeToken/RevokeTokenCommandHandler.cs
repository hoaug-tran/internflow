using InternFlow.Application.Common.Interfaces;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InternFlow.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, ApiResponse<bool>>
{
    private readonly IInternFlowDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public RevokeTokenCommandHandler(
        IInternFlowDbContext context,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<ApiResponse<bool>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtTokenService.HashToken(request.RefreshToken);

        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash, cancellationToken);

        if (token == null)
        {
            return ApiResponse<bool>.Fail("Token không tồn tại");
        }

        if (token.RevokedAt == null)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = request.Reason ?? "Người dùng chủ động đăng xuất";
            await _context.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<bool>.Ok(true, "Thu hồi token thành công");
    }
}

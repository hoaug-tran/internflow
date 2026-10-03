using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Auth.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = InternFlow.Domain.Identity.RefreshToken;

namespace InternFlow.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponse>>
{
    private readonly IInternFlowDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(
        IInternFlowDbContext context,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtTokenService.HashToken(request.RefreshToken);

        var token = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == tokenHash, cancellationToken);

        if (token == null)
        {
            return ApiResponse<AuthResponse>.Fail("Refresh Token không hợp lệ");
        }

        // Token Rotation Security: Phát hiện tái sử dụng Token đã dùng hoặc đã thu hồi
        if (token.RevokedAt != null || token.UsedAt != null)
        {
            var familyTokens = await _context.RefreshTokens
                .Where(r => r.FamilyId == token.FamilyId && r.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var t in familyTokens)
            {
                t.RevokedAt = DateTime.UtcNow;
                t.RevokedReason = "Phát hiện tái sử dụng Refresh Token (Thu hồi toàn bộ chuỗi token)";
            }

            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<AuthResponse>.Fail("Phiên đăng nhập bị xâm phạm hoặc đã hết hạn. Vui lòng đăng nhập lại.");
        }

        // Kiểm tra hết hạn
        if (token.ExpiresAt < DateTime.UtcNow)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = "Token đã hết hạn";
            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<AuthResponse>.Fail("Refresh Token đã hết hạn. Vui lòng đăng nhập lại.");
        }

        if (!token.User.IsActive)
        {
            return ApiResponse<AuthResponse>.Fail("Tài khoản người dùng đã bị vô hiệu hóa.");
        }

        // Đánh dấu token hiện tại đã sử dụng
        token.UsedAt = DateTime.UtcNow;

        // Sinh cặp token mới cùng FamilyId
        var (accessToken, accessExpiresAt) = _jwtTokenService.GenerateAccessToken(token.User);
        var (newRawRt, newRtHash, _, newRtExpiresAt) = _jwtTokenService.GenerateRefreshToken(token.FamilyId);

        var newRtEntity = new RefreshTokenEntity
        {
            UserId = token.UserId,
            TokenHash = newRtHash,
            FamilyId = token.FamilyId,
            ExpiresAt = newRtExpiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = request.IpAddress,
            UserAgent = request.UserAgent
        };

        await _context.RefreshTokens.AddAsync(newRtEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        token.ReplacedByTokenId = newRtEntity.Id;
        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRawRt,
            AccessTokenExpiresAt = accessExpiresAt,
            User = new UserInfoResponse
            {
                Id = token.User.Id,
                Email = token.User.Email,
                FullName = token.User.FullName,
                Role = token.User.Role.ToString()
            }
        };

        return ApiResponse<AuthResponse>.Ok(response, "Cấp lại token thành công");
    }
}

using InternFlow.Application.Common.Interfaces;
using InternFlow.Application.Features.Auth.DTOs;
using InternFlow.Domain.Identity;
using InternFlow.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = InternFlow.Domain.Identity.RefreshToken;

namespace InternFlow.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponse>>
{
    private readonly IInternFlowDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IInternFlowDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (user == null || !user.IsActive)
        {
            return ApiResponse<AuthResponse>.Fail("Email hoặc mật khẩu không chính xác");
        }

        // Kiểm tra tạm khóa tài khoản
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
        {
            var remaining = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
            return ApiResponse<AuthResponse>.Fail($"Tài khoản đang bị tạm khóa. Vui lòng thử lại sau {remaining} phút.");
        }

        // Kiểm tra mật khẩu
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            }
            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<AuthResponse>.Fail("Email hoặc mật khẩu không chính xác");
        }

        // Reset trạng thái lockout
        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;

        // Sinh token
        var (accessToken, accessExpiresAt) = _jwtTokenService.GenerateAccessToken(user);
        var (rawRefreshToken, tokenHash, familyId, refreshExpiresAt) = _jwtTokenService.GenerateRefreshToken();

        var rtEntity = new RefreshTokenEntity
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            FamilyId = familyId,
            ExpiresAt = refreshExpiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = request.IpAddress,
            UserAgent = request.UserAgent
        };

        await _context.RefreshTokens.AddAsync(rtEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            AccessTokenExpiresAt = accessExpiresAt,
            User = new UserInfoResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString()
            }
        };

        return ApiResponse<AuthResponse>.Ok(response, "Đăng nhập thành công");
    }
}

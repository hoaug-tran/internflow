using InternFlow.Application.Features.Auth.Commands.RefreshToken;
using InternFlow.Domain.Identity;
using InternFlow.Infrastructure.Data;
using InternFlow.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using RefreshTokenEntity = InternFlow.Domain.Identity.RefreshToken;

namespace InternFlow.Tests.Auth;

public class RefreshTokenTests : IDisposable
{
    private readonly InternFlowDbContext _context;
    private readonly JwtTokenService _jwtService;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenTests()
    {
        var options = new DbContextOptionsBuilder<InternFlowDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new InternFlowDbContext(options);

        var configValues = new Dictionary<string, string?>
        {
            { "Jwt:Secret", "Super_Secret_Key_For_Unit_Testing_Must_Be_Long_Enough_2026!" },
            { "Jwt:Issuer", "InternFlowTest" },
            { "Jwt:Audience", "InternFlowTestClient" },
            { "Jwt:AccessTokenExpirationMinutes", "15" },
            { "Jwt:RefreshTokenExpirationDays", "7" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _jwtService = new JwtTokenService(configuration);
        _handler = new RefreshTokenCommandHandler(_context, _jwtService);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldRotateTokenSuccessfully()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "intern@internflow.com",
            FullName = "Test Intern",
            PasswordHash = "hash",
            Role = UserRole.Intern,
            IsActive = true
        };
        _context.Users.Add(user);

        var (rawRt, rtHash, familyId, expiresAt) = _jwtService.GenerateRefreshToken();
        var originalRtEntity = new RefreshTokenEntity
        {
            UserId = user.Id,
            TokenHash = rtHash,
            FamilyId = familyId,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = "127.0.0.1",
            UserAgent = "TestAgent"
        };
        _context.RefreshTokens.Add(originalRtEntity);
        await _context.SaveChangesAsync();

        var command = new RefreshTokenCommand(rawRt, "127.0.0.1", "TestAgent");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data.AccessToken);
        Assert.NotEmpty(result.Data.RefreshToken);
        Assert.NotEqual(rawRt, result.Data.RefreshToken);

        // Verify old token is marked as used
        var oldTokenInDb = await _context.RefreshTokens.FirstAsync(r => r.Id == originalRtEntity.Id);
        Assert.NotNull(oldTokenInDb.UsedAt);
        Assert.NotNull(oldTokenInDb.ReplacedByTokenId);

        // Verify new token exists in same family
        var newHash = _jwtService.HashToken(result.Data.RefreshToken);
        var newTokenInDb = await _context.RefreshTokens.FirstAsync(r => r.TokenHash == newHash);
        Assert.Equal(familyId, newTokenInDb.FamilyId);
        Assert.Null(newTokenInDb.UsedAt);
        Assert.Null(newTokenInDb.RevokedAt);
    }

    [Fact]
    public async Task Handle_WithExpiredRefreshToken_ShouldFailAndMarkRevoked()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "intern@internflow.com",
            FullName = "Test Intern",
            PasswordHash = "hash",
            Role = UserRole.Intern,
            IsActive = true
        };
        _context.Users.Add(user);

        var (rawRt, rtHash, familyId, _) = _jwtService.GenerateRefreshToken();
        var expiredEntity = new RefreshTokenEntity
        {
            UserId = user.Id,
            TokenHash = rtHash,
            FamilyId = familyId,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5), // Expired!
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            CreatedByIp = "127.0.0.1",
            UserAgent = "TestAgent"
        };
        _context.RefreshTokens.Add(expiredEntity);
        await _context.SaveChangesAsync();

        var command = new RefreshTokenCommand(rawRt, "127.0.0.1", "TestAgent");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("hết hạn", result.Message, StringComparison.OrdinalIgnoreCase);

        var tokenInDb = await _context.RefreshTokens.FirstAsync(r => r.Id == expiredEntity.Id);
        Assert.NotNull(tokenInDb.RevokedAt);
        Assert.Equal("Token đã hết hạn", tokenInDb.RevokedReason);
    }

    [Fact]
    public async Task Handle_WhenReusingAlreadyUsedToken_ShouldTriggerReuseDetection_AndRevokeAllFamilyTokens()
    {
        // Arrange - Simulate a compromised token chain
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "intern@internflow.com",
            FullName = "Test Intern",
            PasswordHash = "hash",
            Role = UserRole.Intern,
            IsActive = true
        };
        _context.Users.Add(user);

        var familyId = Guid.NewGuid();

        // 1. Compromised old token (already used)
        var (rawUsedRt, usedRtHash, _, _) = _jwtService.GenerateRefreshToken(familyId);
        var usedToken = new RefreshTokenEntity
        {
            UserId = user.Id,
            TokenHash = usedRtHash,
            FamilyId = familyId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            UsedAt = DateTime.UtcNow.AddMinutes(-5), // ALREADY USED
            CreatedByIp = "127.0.0.1"
        };

        // 2. Active child token in same family
        var (_, activeRtHash, _, _) = _jwtService.GenerateRefreshToken(familyId);
        var activeChildToken = new RefreshTokenEntity
        {
            UserId = user.Id,
            TokenHash = activeRtHash,
            FamilyId = familyId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            CreatedByIp = "127.0.0.1"
        };

        _context.RefreshTokens.AddRange(usedToken, activeChildToken);
        await _context.SaveChangesAsync();

        // An attacker (or replayed request) tries to use the already-used token
        var command = new RefreshTokenCommand(rawUsedRt, "192.168.1.100", "AttackerAgent");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("xâm phạm", result.Message, StringComparison.OrdinalIgnoreCase);

        // Verify that ALL tokens in the family are now revoked!
        var childTokenInDb = await _context.RefreshTokens.FirstAsync(r => r.Id == activeChildToken.Id);
        Assert.NotNull(childTokenInDb.RevokedAt);
        Assert.Contains("tái sử dụng", childTokenInDb.RevokedReason);
    }
}

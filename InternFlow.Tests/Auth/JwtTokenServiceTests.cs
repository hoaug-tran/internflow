using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using InternFlow.Domain.Identity;
using InternFlow.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace InternFlow.Tests.Auth;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _tokenService;

    public JwtTokenServiceTests()
    {
        var configValues = new Dictionary<string, string?>
        {
            { "Jwt:Secret", "Super_Secret_Key_For_Unit_Testing_Must_Be_Long_Enough_2026!" },
            { "Jwt:Issuer", "InternFlowTestIssuer" },
            { "Jwt:Audience", "InternFlowTestAudience" },
            { "Jwt:AccessTokenExpirationMinutes", "15" },
            { "Jwt:RefreshTokenExpirationDays", "7" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _tokenService = new JwtTokenService(configuration);
    }

    [Fact]
    public void GenerateAccessToken_ShouldProduceValidJwt_WithExpectedClaims()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "intern.test@internflow.com",
            FullName = "Nguyen Van Test",
            Role = UserRole.Intern
        };

        // Act
        var (token, expiresAt) = _tokenService.GenerateAccessToken(user);

        // Assert
        Assert.NotEmpty(token);
        Assert.True(expiresAt > DateTime.UtcNow);

        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));

        var jwt = handler.ReadJwtToken(token);
        Assert.Equal("InternFlowTestIssuer", jwt.Issuer);

        var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;
        var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;

        Assert.Equal(user.Id.ToString(), subClaim);
        Assert.Equal(UserRole.Intern.ToString(), roleClaim);
        Assert.Equal(user.Email, emailClaim);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldGenerateValidTokens_AndKeepFamilyId()
    {
        // Arrange
        var existingFamilyId = Guid.NewGuid();

        // Act
        var (rawToken, tokenHash, familyId, expiresAt) = _tokenService.GenerateRefreshToken(existingFamilyId);

        // Assert
        Assert.NotEmpty(rawToken);
        Assert.NotEmpty(tokenHash);
        Assert.Equal(64, tokenHash.Length); // SHA-256 in hex format is 64 chars
        Assert.Equal(existingFamilyId, familyId);
        Assert.True(expiresAt > DateTime.UtcNow.AddDays(6));
    }

    [Fact]
    public void GenerateRefreshToken_WhenNoFamilyIdPassed_ShouldGenerateNewFamilyId()
    {
        // Act
        var (_, _, familyId, _) = _tokenService.GenerateRefreshToken(null);

        // Assert
        Assert.NotEqual(Guid.Empty, familyId);
    }

    [Fact]
    public void HashToken_ShouldBeDeterministic()
    {
        // Arrange
        var raw = "SampleRawTokenString123!";

        // Act
        var hash1 = _tokenService.HashToken(raw);
        var hash2 = _tokenService.HashToken(raw);

        // Assert
        Assert.Equal(hash1, hash2);
        Assert.Equal(hash1.ToLowerInvariant(), hash1);
    }
}

using FluentAssertions;
using InternFlow.Application.Features.Auth.Commands.Login;
using InternFlow.Application.Features.Auth.Commands.RefreshToken;
using InternFlow.Application.Features.Auth.Commands.RevokeToken;
using Xunit;

namespace InternFlow.Tests.Auth;

public class AuthValidationTests
{
    private readonly LoginCommandValidator _loginValidator = new();
    private readonly RefreshTokenCommandValidator _refreshTokenValidator = new();
    private readonly RevokeTokenCommandValidator _revokeTokenValidator = new();

    #region LoginCommandValidator Tests

    [Fact]
    public void Login_ValidCommand_ShouldPassValidation()
    {
        var command = new LoginCommand("admin@internflow.com", "Admin@123456");

        var result = _loginValidator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Login_EmptyEmail_ShouldFailValidation(string email)
    {
        var command = new LoginCommand(email, "Password123!");

        var result = _loginValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("admin@")]
    [InlineData("@internflow.com")]
    public void Login_InvalidEmailFormat_ShouldFailValidation(string email)
    {
        var command = new LoginCommand(email, "Password123!");

        var result = _loginValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("email"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void Login_EmptyPassword_ShouldFailValidation(string password)
    {
        var command = new LoginCommand("admin@internflow.com", password);

        var result = _loginValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    #endregion

    #region RefreshTokenCommandValidator Tests

    [Fact]
    public void RefreshToken_ValidToken_ShouldPassValidation()
    {
        var command = new RefreshTokenCommand("valid-refresh-token-xyz");

        var result = _refreshTokenValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void RefreshToken_EmptyToken_ShouldFailValidation(string token)
    {
        var command = new RefreshTokenCommand(token);

        var result = _refreshTokenValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RefreshToken");
    }

    #endregion

    #region RevokeTokenCommandValidator Tests

    [Fact]
    public void RevokeToken_ValidCommand_ShouldPassValidation()
    {
        var command = new RevokeTokenCommand("valid-refresh-token", "Logout normal");

        var result = _revokeTokenValidator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null!)]
    public void RevokeToken_EmptyToken_ShouldFailValidation(string token)
    {
        var command = new RevokeTokenCommand(token);

        var result = _revokeTokenValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RefreshToken");
    }

    [Fact]
    public void RevokeToken_ReasonTooLong_ShouldFailValidation()
    {
        var longReason = new string('a', 501);
        var command = new RevokeTokenCommand("valid-token", longReason);

        var result = _revokeTokenValidator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Reason");
    }

    #endregion
}

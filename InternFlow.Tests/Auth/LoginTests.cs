using InternFlow.Application.Features.Auth.Commands.Login;
using InternFlow.Domain.Identity;
using InternFlow.Infrastructure.Data;
using InternFlow.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace InternFlow.Tests.Auth;

public class LoginTests : IDisposable
{
    private readonly InternFlowDbContext _context;
    private readonly PasswordHasher _hasher;
    private readonly JwtTokenService _jwtService;
    private readonly LoginCommandHandler _handler;

    public LoginTests()
    {
        var options = new DbContextOptionsBuilder<InternFlowDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new InternFlowDbContext(options);
        _hasher = new PasswordHasher();

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
        _handler = new LoginCommandHandler(_context, _hasher, _jwtService);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldLoginSuccessfully()
    {
        // Arrange
        var password = "CorrectPassword123!";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@internflow.com",
            FullName = "Administrator",
            PasswordHash = _hasher.HashPassword(password),
            Role = UserRole.Admin,
            IsActive = true,
            FailedLoginCount = 2
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new LoginCommand("admin@internflow.com", password, "127.0.0.1", "BrowserTest");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data.AccessToken);
        Assert.NotEmpty(result.Data.RefreshToken);
        Assert.Equal("admin@internflow.com", result.Data.User.Email);
        Assert.Equal("Admin", result.Data.User.Role);

        // Verify lockout state reset & LastLoginAt set
        var userInDb = await _context.Users.FirstAsync(u => u.Id == user.Id);
        Assert.Equal(0, userInDb.FailedLoginCount);
        Assert.Null(userInDb.LockoutEnd);
        Assert.NotNull(userInDb.LastLoginAt);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldIncrementFailedCount()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "mentor@internflow.com",
            FullName = "Mentor User",
            PasswordHash = _hasher.HashPassword("CorrectPassword!"),
            Role = UserRole.Mentor,
            IsActive = true,
            FailedLoginCount = 0
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new LoginCommand("mentor@internflow.com", "WrongPassword!", "127.0.0.1", "BrowserTest");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        var userInDb = await _context.Users.FirstAsync(u => u.Id == user.Id);
        Assert.Equal(1, userInDb.FailedLoginCount);
        Assert.Null(userInDb.LockoutEnd);
    }

    [Fact]
    public async Task Handle_AfterFiveConsecutiveFailedLogins_ShouldLockAccount()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "brute.force@internflow.com",
            FullName = "Target User",
            PasswordHash = _hasher.HashPassword("RealPassword!"),
            Role = UserRole.Intern,
            IsActive = true,
            FailedLoginCount = 4 // Already failed 4 times
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new LoginCommand("brute.force@internflow.com", "WrongPassword5th!", "127.0.0.1", "BrowserTest");

        // Act - 5th failure
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        var userInDb = await _context.Users.FirstAsync(u => u.Id == user.Id);
        Assert.Equal(5, userInDb.FailedLoginCount);
        Assert.NotNull(userInDb.LockoutEnd);
        Assert.True(userInDb.LockoutEnd > DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_WhenAccountIsLocked_ShouldRejectImmediately()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "locked@internflow.com",
            FullName = "Locked User",
            PasswordHash = _hasher.HashPassword("CorrectPassword!"),
            Role = UserRole.Intern,
            IsActive = true,
            LockoutEnd = DateTime.UtcNow.AddMinutes(10)
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var command = new LoginCommand("locked@internflow.com", "CorrectPassword!", "127.0.0.1", "BrowserTest");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("tạm khóa", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}

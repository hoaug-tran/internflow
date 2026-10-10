using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using InternFlow.Application.Features.Auth.DTOs;
using InternFlow.Application.Features.Internships.DTOs;
using InternFlow.Shared.Common;
using Xunit;

namespace InternFlow.Tests.Integration;

public class IdentityAndInternshipIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public IdentityAndInternshipIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var loginRequest = new LoginRequest
        {
            Email = "admin@internflow.com",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        return result.Data!.AccessToken;
    }

    private async Task<string> GetInternTokenAsync()
    {
        var loginRequest = new LoginRequest
        {
            Email = "intern@internflow.com",
            Password = "Password123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        return result.Data!.AccessToken;
    }

    [Fact]
    public async Task Login_WithValidAdminCredentials_Returns200AndValidTokens()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "admin@internflow.com",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();
        body.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.Data.User.Email.Should().Be("admin@internflow.com");
        body.Data.User.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsErrorResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "admin@internflow.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeFalse();
        body.Message.Should().Contain("không chính xác");
    }

    [Fact]
    public async Task GetCurrentUser_WithValidBearerToken_Returns200AndUserInfo()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(requestMessage);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserInfoResponse>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();
        body.Data!.Email.Should().Be("admin@internflow.com");
        body.Data.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_Returns401Unauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateInternship_WithAdminRole_Returns200AndInternshipData()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/internships")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(new CreateInternshipRequest(
                "Đợt Thực Tập Tích Hợp Test 2026",
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3))))
        };

        // Act
        var response = await _client.SendAsync(requestMessage);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<InternshipDto>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();
        body.Data!.Id.Should().NotBeEmpty();
        body.Data.Name.Should().Be("Đợt Thực Tập Tích Hợp Test 2026");
    }

    [Fact]
    public async Task CreateInternship_WithInternRole_Returns403Forbidden()
    {
        // Arrange
        var internToken = await GetInternTokenAsync();
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/internships")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", internToken) },
            Content = JsonContent.Create(new CreateInternshipRequest(
                "Đợt Thực Tập Bị Chặn 2026",
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3))))
        };

        // Act
        var response = await _client.SendAsync(requestMessage);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateInternship_WithInvalidDateRange_Returns400BadRequest()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/internships")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(new CreateInternshipRequest(
                "Đợt Lỗi Ngày",
                DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)), // Start date sau End date
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))))
        };

        // Act
        var response = await _client.SendAsync(requestMessage);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AssignMentor_FullWorkflow_SuccessAndIdempotent()
    {
        // Arrange: Admin token & tạo mới 1 đợt thực tập
        var token = await GetAdminTokenAsync();

        // 1. Tạo đợt thực tập
        using var createBatchMsg = new HttpRequestMessage(HttpMethod.Post, "/api/internships")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(new CreateInternshipRequest(
                "Đợt Test Phân Công Mentor",
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(2))))
        };
        var createBatchRes = await _client.SendAsync(createBatchMsg);
        createBatchRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var batchBody = await createBatchRes.Content.ReadFromJsonAsync<ApiResponse<InternshipDto>>();
        var batchId = batchBody!.Data!.Id;

        // Lấy thông tin Mentor và Intern từ seed data
        var mentorUserRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "mentor@internflow.com",
            Password = "Password123!"
        });
        var mentorBody = await mentorUserRes.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        var mentorId = mentorBody!.Data!.User.Id;

        var internUserRes = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = "intern@internflow.com",
            Password = "Password123!"
        });
        var internBody = await internUserRes.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        var internId = internBody!.Data!.User.Id;

        // 2. Thêm Mentor vào đợt
        using var addMentorMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/internships/{batchId}/mentors")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(new AddBatchMentorRequest(mentorId))
        };
        var addMentorRes = await _client.SendAsync(addMentorMsg);
        addMentorRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Thêm Intern vào đợt
        using var addInternMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/internships/{batchId}/interns")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(new AddBatchInternRequest(internId, "Ghi chú test tích hợp"))
        };
        var addInternRes = await _client.SendAsync(addInternMsg);
        addInternRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Phân công Mentor cho Intern
        using var assignMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/internships/{batchId}/assignments")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(new AssignMentorRequest(internId, mentorId))
        };
        var assignRes = await _client.SendAsync(assignMsg);

        // Assert
        assignRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignBody = await assignRes.Content.ReadFromJsonAsync<ApiResponse<long>>();
        assignBody.Should().NotBeNull();
        assignBody!.Success.Should().BeTrue();
        assignBody.Data.Should().BeGreaterThan(0);

        // 5. Thử phân công lại cùng Mentor cho cùng Intern -> Kiểm tra tính duy nhất (Idempotent / Uniqueness)
        using var reassignMsg = new HttpRequestMessage(HttpMethod.Post, $"/api/internships/{batchId}/assignments")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(new AssignMentorRequest(internId, mentorId))
        };
        var reassignRes = await _client.SendAsync(reassignMsg);
        reassignRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var reassignBody = await reassignRes.Content.ReadFromJsonAsync<ApiResponse<long>>();
        reassignBody!.Data.Should().Be(assignBody.Data); // Trả về cùng assignment id, không duplicate
    }
}

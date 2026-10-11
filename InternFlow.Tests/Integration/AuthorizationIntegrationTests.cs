using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace InternFlow.Tests.Integration;

[Collection("Integration")]
public class AuthorizationIntegrationTests : IntegrationTestBase
{
    public AuthorizationIntegrationTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    // ═══════════════════════════════════════════════════
    // 401 — Chưa đăng nhập
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task GetProjects_NoToken_Returns401()
    {
        var response = await Client.GetAsync("/api/projects");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProject_NoToken_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/projects", new
        {
            internshipId = Guid.NewGuid(),
            name = "X"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTaskSummary_NoToken_Returns401()
    {
        var response = await Client.GetAsync(
            $"/api/projects/{Guid.NewGuid()}/tasks/summary");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ═══════════════════════════════════════════════════
    // 404 — Project không tồn tại
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task GetProject_NotFound_Returns404()
    {
        await AuthenticateAsync();
        var fakeId = Guid.NewGuid();

        // ★ Nếu controller route khác (ví dụ /api/project), đổi URL tại đây
        var response = await Client.GetAsync($"/api/projects/{fakeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTaskSummary_ProjectNotFound_Returns404()
    {
        await AuthenticateAsync();
        var fakeId = Guid.NewGuid();

        var response = await Client.GetAsync(
            $"/api/projects/{fakeId}/tasks/summary");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ═══════════════════════════════════════════════════
    // 400 — Validation
    // ═══════════════════════════════════════════════════

    [Fact]
    public async Task CreateProject_EmptyName_Returns400()
    {
        await AuthenticateAsync();

        var response = await Client.PostAsJsonAsync("/api/projects", new
        {
            internshipId = Guid.NewGuid(),
            name = "",
            description = "X",
            startDate = (string?)null,
            endDate = (string?)null
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProject_EndDateBeforeStartDate_Returns400()
    {
        await AuthenticateAsync();

        var response = await Client.PostAsJsonAsync("/api/projects", new
        {
            internshipId = Guid.NewGuid(),
            name = "Test",
            description = "X",
            startDate = "2026-12-31",
            endDate = "2026-01-01"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

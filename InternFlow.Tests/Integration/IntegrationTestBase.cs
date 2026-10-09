using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using InternFlow.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace InternFlow.Tests.Integration;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<string> AuthenticateAsync(
        string email = "admin@internflow.com",
        string password = "Password123!")
    {
        var response = await Client.PostAsJsonAsync("/api/Auth/login", new
        {
            email,
            password
        });

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Xunit.Sdk.XunitException(
                $"Login failed: {(int)response.StatusCode} {response.StatusCode}\nBody: {body}");
        }

        var content = JsonSerializer.Deserialize<LoginEnvelope>(body, JsonOptions);

        if (content?.Data?.AccessToken is null)
        {
            throw new Xunit.Sdk.XunitException(
                $"Login response shape mismatch.\nBody: {body}");
        }

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", content.Data.AccessToken);

        return content.Data.AccessToken;
    }

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();

    protected InternFlowDbContext CreateDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<InternFlowDbContext>();
    }

    private sealed record LoginEnvelope(LoginData Data);
    private sealed record LoginData(string AccessToken);
}
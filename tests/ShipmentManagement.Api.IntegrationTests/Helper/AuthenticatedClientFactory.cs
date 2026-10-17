using System.Net.Http.Headers;
using System.Net.Http.Json;

using ShipmentManagement.Api.IntegrationTests.TestHost;
using ShipmentManagement.Application.DTOs.Auth;

namespace ShipmentManagement.Api.IntegrationTests.Helper;

public static class AuthenticatedClientFactory
{
    public static async Task<HttpClient> CreateAsync(CustomWebApplicationFactory factory)
    {
        var client = factory.CreateClient();

        var loginRequest = new LoginRequestDto { Email = "testuser@example.com", Password = "UserPassword123" };
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        loginResponse.EnsureSuccessStatusCode();

        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponseDto>();

        if (tokens is null || string.IsNullOrWhiteSpace(tokens.AccessToken))
        {
            client.Dispose();
            throw new InvalidOperationException("Login did not return an access token.");
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        return client;
    }

    public static async Task<HttpClient> RegisterAndCreateAsync(CustomWebApplicationFactory factory)
    {
        var client = factory.CreateClient();

        var registerRequest = new RegisterRequestDto
        {
            UserName = "newtestuser@example.com",
            Email = "newtestuser@example.com",
            Password = "new123456",
            ConfirmPassword = "new123456"
        };
        var registerResponse = await client.PostAsJsonAsync("api/v1.0/Auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        
        var loginRequest = new LoginRequestDto { Email = "newtestuser@example.com", Password = "new123456" };
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        loginResponse.EnsureSuccessStatusCode();

        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponseDto>();

        if (tokens is null || string.IsNullOrWhiteSpace(tokens.AccessToken))
        {
            client.Dispose();
            throw new InvalidOperationException("Login did not return an access token.");
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        return client;
    }

    public static async Task<TokenResponseDto> GetAuthTokensAsync(CustomWebApplicationFactory factory)
    {
        var client = factory.CreateClient();

        var loginRequest = new LoginRequestDto { Email = "testuser@example.com", Password = "UserPassword123" };
        var loginResponse = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        loginResponse.EnsureSuccessStatusCode();

        var tokens = await loginResponse.Content.ReadFromJsonAsync<TokenResponseDto>();

        client.Dispose();

        if (tokens is null || string.IsNullOrWhiteSpace(tokens.AccessToken))
        {
            throw new InvalidOperationException("Login did not return an access token.");
        }

        return tokens;
    }
}
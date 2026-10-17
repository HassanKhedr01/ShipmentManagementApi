using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using FluentValidation;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using ShipmentManagement.Api.IntegrationTests.Common;
using ShipmentManagement.Api.IntegrationTests.Helper;
using ShipmentManagement.Api.IntegrationTests.TestHost;
using ShipmentManagement.Application.DTOs.Auth;
using ShipmentManagement.Infrastructure;
using ShipmentManagement.Infrastructure.Identity;

namespace ShipmentManagement.Api.IntegrationTests.Controllers;

public class AuthControllerTests
{
    #region Register

    [Fact]
    public async Task Register_ValidRegisterRequest_ReturnsOKStatusCode()
    {
        // Arrange 
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var request = new RegisterRequestDto
        {
            UserName = "TestName", Email = "test@example.com", Password = "123456", ConfirmPassword = "123456"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1.0/auth/register", request);
        var action = () => response.EnsureSuccessStatusCode();

        // Assert
        action.Should().NotThrow();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        var message = messageResponse?.Message;
        message.Should().NotBeNull();
        message.Should().Be("User registered successfully.");
        var user = await userManager.FindByEmailAsync(request.Email);
        user.Should().NotBeNull();
        user.Email.Should().Be(request.Email);
        user.SecurityStamp.Should().NotBeNullOrWhiteSpace();
        user.PasswordHash.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("", "", "", "")]
    [InlineData("", "test@example.com", "123456", "123456")]
    [InlineData("testuser", "test.com", "123456", "123456")]
    [InlineData("testuser", "@example.com", "123456", "123456")]
    [InlineData("testuser", "789", "123456", "123456")]
    [InlineData("testuser", "test@example.com", "", "123456")]
    [InlineData("testuser", "test@example.com", "123456", "654321")]
    [InlineData("testuser", "test@example.com", "12", "12")]
    public async Task Register_RequestWithValidationErrors_ReturnsBadRequestWithError(
        string userName,
        string email,
        string password,
        string confirmPassword)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var request = new RegisterRequestDto
        {
            UserName = userName, Email = email, Password = password, ConfirmPassword = confirmPassword
        };
        var validator = factory.Services.GetRequiredService<IValidator<RegisterRequestDto>>();
        var validationResult = await validator.ValidateAsync(request);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1.0/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errors = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>();
        errors.Should().NotBeNull();
        errors.Should().NotBeEmpty();
        errors.Should().BeEquivalentTo(validationResult.ToDictionary());
    }

    [Theory]
    [InlineData("test user", "test@example.com", "123456", "123456")] // invalid user name
    [InlineData("testuser", "testuser@example.com", "123456", "123456")] // email already exists
    public async Task Register_RegistrationFailed_ReturnsBadRequestWithErrors(
        string userName,
        string email,
        string password,
        string confirmPassword)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var request = new RegisterRequestDto
        {
            UserName = userName, Email = email, Password = password, ConfirmPassword = confirmPassword
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1.0/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errors = await response.Content.ReadFromJsonAsync<List<string>>();
        errors.Should().NotBeNull();
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_AccessedWhileAuthenticated_ReturnsForbidden()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1.0/auth/register", new RegisterRequestDto());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var problems = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problems.Should().NotBeNull();
        problems.Title.Should().Be("Forbidden");
        problems.Detail.Should().Be("This endpoint is restricted to unauthenticated guests only.");
    }

    #endregion

    #region Login

    [Fact]
    public async Task Login_AccessedWhileAuthenticated_ReturnsForbidden()
    {
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1.0/auth/login", new LoginRequestDto());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var problems = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problems.Should().NotBeNull();
        problems.Title.Should().Be("Forbidden");
        problems.Detail.Should().Be("This endpoint is restricted to unauthenticated guests only.");
    }

    [Fact]
    public async Task Login_ValidLoggingRequest_ReturnsOKStatusCodeWithTheTokens()
    {
        // Arrange 
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var jwtOptions = factory.Services.GetRequiredService<IOptions<JwtOptions>>().Value;

        var request = new LoginRequestDto { Email = "testuser@example.com", Password = "UserPassword123" };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1.0/auth/login", request);
        var action = () => response.EnsureSuccessStatusCode();

        // Assert
        action.Should().NotThrow();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
        tokens.Should().NotBeNull();
        tokens.AccessToken.Should().NotBeNullOrEmpty();
        tokens.RefreshToken.Should().NotBeNullOrEmpty();
        tokens.AccessTokenExpiresAt.Should()
            .BeCloseTo(DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenExpirationMinutes),
                TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("test@example.com", "")]
    [InlineData("", "123456")]
    [InlineData("@example.com", "123456")]
    [InlineData("test.com", "123456")]
    [InlineData("test@exaple.com", "123")]
    public async Task Login_RequestWithValidationErrors_ReturnsBadRequestWithError(string email, string password)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var request = new LoginRequestDto { Email = email, Password = password };
        var validator = factory.Services.GetRequiredService<IValidator<LoginRequestDto>>();
        var validationResult = await validator.ValidateAsync(request);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1.0/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errors = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>();
        errors.Should().NotBeNull();
        errors.Should().NotBeEmpty();
        errors.Should().BeEquivalentTo(validationResult.ToDictionary());
    }

    [Theory]
    [InlineData("emailnonexist@exaple.com", "123456")] // email does not exist
    [InlineData("testuser@example.com", "987654321ABC")] // invalid password
    public async Task Login_LoginFailed_ReturnsUnauthorized(string email, string password)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var request = new LoginRequestDto { Email = email, Password = password };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1.0/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Refresh

    [Fact]
    public async Task Refresh_ValidRefreshRequest_ReturnsOKStatusCodeWithTheTokens()
    {
        // Arrange 
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var oldTokens = await AuthenticatedClientFactory.GetAuthTokensAsync(factory);
        var jwtOptions = factory.Services.GetRequiredService<IOptions<JwtOptions>>().Value;
        ////
        var request = new RefreshRequestDto
        {
            ExpiredAccessToken = oldTokens.AccessToken, RefreshToken = oldTokens.RefreshToken
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1.0/auth/refresh", request);
        var action = () => response.EnsureSuccessStatusCode();

        // Assert
        action.Should().NotThrow();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokens = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
        tokens.Should().NotBeNull();
        tokens.AccessToken.Should().NotBeNullOrEmpty();
        tokens.RefreshToken.Should().NotBeNullOrEmpty();
        tokens.Should().NotBe(oldTokens);
        tokens.AccessTokenExpiresAt.Should()
            .BeCloseTo(DateTime.UtcNow.AddMinutes(jwtOptions.AccessTokenExpirationMinutes),
                TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData("EXPIRED_TOKEN", "")]
    [InlineData("", "REFRESH_TOKEN")]
    public async Task Refresh_RequestWithValidationErrors_ReturnsBadRequestWithError(
        string expiredToken,
        string refreshToken)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var request = new RefreshRequestDto { ExpiredAccessToken = expiredToken, RefreshToken = refreshToken };
        var validator = factory.Services.GetRequiredService<IValidator<RefreshRequestDto>>();
        var validationResult = await validator.ValidateAsync(request);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1.0/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errors = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>();
        errors.Should().NotBeNull();
        errors.Should().NotBeEmpty();
        errors.Should().BeEquivalentTo(validationResult.ToDictionary());
    }

    [Fact]
    public async Task Refresh_InvalidExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var oldTokens = await AuthenticatedClientFactory.GetAuthTokensAsync(factory);
        oldTokens.AccessToken = "INVALID_ACCESS_TOKEN";
        var request = new RefreshRequestDto
        {
            ExpiredAccessToken = oldTokens.AccessToken, RefreshToken = oldTokens.RefreshToken
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1.0/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_InvalidRefreshToken_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var oldTokens = await AuthenticatedClientFactory.GetAuthTokensAsync(factory);
        oldTokens.RefreshToken = "INVALID_REFRESH_TOKEN";
        var request = new RefreshRequestDto
        {
            ExpiredAccessToken = oldTokens.AccessToken, RefreshToken = oldTokens.RefreshToken
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/v1.0/auth/refresh", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Logout

    [Fact]
    public async Task Logout_RequestWhileNonAuthenticated_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        
        // Act
        var response = await client.PostAsJsonAsync("api/v1.0/Auth/logout", new LogoutRequestDto());
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task Logout_ValidLogoutRequest_ReturnsNoContent()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);
        var tokens = await AuthenticatedClientFactory.GetAuthTokensAsync(factory);
        var request = new LogoutRequestDto { RefreshToken = tokens.RefreshToken };
        
        // Act
        var response = await client.PostAsJsonAsync("api/v1.0/Auth/logout",request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    
    [Fact]
    public async Task Logout_RequestWithValidationErrors_ReturnsBadRequestWithError()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);
        var request = new LogoutRequestDto { RefreshToken = "" };
        var validator = factory.Services.GetRequiredService<IValidator<LogoutRequestDto>>();
        var validationResult = await validator.ValidateAsync(request);
        
        // Act
        var response = await client.PostAsJsonAsync("api/v1.0/Auth/logout",request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errors = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>();
        errors.Should().NotBeNull();
        errors.Should().NotBeEmpty();
        errors.Should().BeEquivalentTo(validationResult.ToDictionary());
    }
    
    [Fact]
    public async Task Logout_InvalidRefreshToken_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);
        var request = new LogoutRequestDto { RefreshToken = "INVALID_REFRESH_TOKEN" };
        
        // Act
        var response = await client.PostAsJsonAsync("api/v1.0/Auth/logout",request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        var message = messageResponse?.Message;
        message.Should().NotBeNull();
        message.Should().Be("Failed to log out.");
    }

    #endregion
}
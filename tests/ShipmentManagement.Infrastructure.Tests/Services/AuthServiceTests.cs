using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

using AutoFixture;
using AutoFixture.AutoMoq;

using FluentAssertions;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

using Moq;

using ShipmentManagement.Application.DTOs.Auth;
using ShipmentManagement.Application.Interfaces;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Infrastructure.Identity;
using ShipmentManagement.Infrastructure.Services;

namespace ShipmentManagement.Infrastructure.Tests.Services;

[SuppressMessage("ReSharper", "UseCollectionExpression")]
public class AuthServiceTests
{
    #region RegisterAsync

    [Theory, MemberData(nameof(RegisterCases))]
    public async Task Register_ValidRegistrationRequest_ReturnsSuccessfulAuthenticationResult(
        RegisterRequestDto registerRequest,
        Mock<UserManager<ApplicationUser>> userManagerMock,
        Mock<RoleManager<ApplicationRole>> roleManagerMock,
        AuthService service)
    {
        // Arrange
        var identityResult = IdentityResult.Success;
        userManagerMock.Setup(x => x.FindByEmailAsync(registerRequest.Email)).ReturnsAsync((ApplicationUser?)null);
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerRequest.Password))
            .ReturnsAsync(identityResult);
        userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(identityResult);
        roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        // Act
        var result = await service.RegisterAsync(registerRequest);

        // Assert
        result.Success.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory, MemberData(nameof(RegisterCasesWithExistingIdentityUser))]
    public async Task Register_EmailAlreadyExists_ReturnsSFailedAuthenticationResult(
        ApplicationUser user,
        RegisterRequestDto registerRequest,
        Mock<UserManager<ApplicationUser>> userManagerMock,
        AuthService service)
    {
        // Arrange
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

        // Act
        var result = await service.RegisterAsync(registerRequest);

        // Assert
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain("Invalid Email or Password.");
    }

    [Theory, MemberData(nameof(RegisterCasesWithAccountCreationFailure))]
    public async Task Register_FailureToCreateAUserAccount_ReturnsFailedAuthenticationResult(
        RegisterRequestDto registerRequest,
        Mock<UserManager<ApplicationUser>> userManagerMock,
        AuthService service)
    {
        // Arrange
        var failedResult = IdentityResult.Failed(new IdentityError
        {
            Code = "CreateUserFailed", Description = "Failed to create user."
        });
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerRequest.Password))
            .ReturnsAsync(failedResult);

        // Act
        var result = await service.RegisterAsync(registerRequest);

        // Assert
        result.Success.Should().BeFalse();
        var errors = failedResult.Errors.Select(e => e.Description);
        result.Errors.Should().Contain(errors);
    }

    [Theory, MemberData(nameof(RegisterCases))]
    public async Task Register_FailureToAssignUserToTheRole_ReturnsFailedAuthenticationResult(
        RegisterRequestDto registerRequest,
        Mock<UserManager<ApplicationUser>> userManagerMock,
        Mock<RoleManager<ApplicationRole>> roleManagerMock,
        AuthService service)
    {
        // Arrange
        var failedResult = IdentityResult.Failed(new IdentityError
        {
            Code = "AssignUserToRoleFailed", Description = "Failed to assign the user to the role."
        });
        userManagerMock.Setup(x => x.FindByEmailAsync(registerRequest.Email)).ReturnsAsync((ApplicationUser?)null);
        userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), registerRequest.Password))
            .ReturnsAsync(failedResult);
        userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
            .ReturnsAsync(failedResult);
        roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        // Act
        var result = await service.RegisterAsync(registerRequest);

        // Assert
        result.Success.Should().BeFalse();
        var errors = failedResult.Errors.Select(e => e.Description);
        result.Errors.Should().Contain(errors);
    }

    #endregion

    #region LoginAsync

    [Theory, MemberData(nameof(LoginCases))]
    public async Task Login_ValidLoginRequest_GeneratesTheAccessTokenWithExpirationDateAndTheRefreshToken(
        LoginRequestDto loginRequest,
        JwtTokenDto jwtTokenDto,
        Mock<IJwtService> jwtServiceMock,
        Mock<IRefreshTokenService> refreshTokenMock,
        Mock<UserManager<ApplicationUser>> userManagerMock,
        AuthService service)
    {
        // Arrange
        string refreshToken = new('r', 64);
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((new ApplicationUser()));
        userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string>());
        jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<JwtUserDto>())).Returns(jwtTokenDto);
        refreshTokenMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);
        refreshTokenMock.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns((string hash) => hash);
        refreshTokenMock.Setup(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);

        // Act
        var tokenResponse = await service.LoginAsync(loginRequest);

        // Assert
        tokenResponse.Should().NotBeNull();
        tokenResponse.AccessToken.Should().NotBeNullOrEmpty();
        tokenResponse.RefreshToken.Should().NotBeNullOrEmpty();
        tokenResponse.AccessToken.Should().Be(tokenResponse.AccessToken);
        tokenResponse.RefreshToken.Should().Be(refreshToken);
        tokenResponse.AccessTokenExpiresAt.Should().Be(jwtTokenDto.ExpiresAt);
    }

    [Theory, MemberData(nameof(LoginCasesWithEmailAndPassword))]
    public async Task Login_EmailDoesNotExist_ReturnsNull(
        LoginRequestDto loginRequest,
        Mock<UserManager<ApplicationUser>> userManagerMock,
        AuthService service)
    {
        // Arrange
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        // Act
        var tokenResponse = await service.LoginAsync(loginRequest);

        // Assert
        tokenResponse.Should().BeNull();
    }

    [Theory, MemberData(nameof(LoginCasesWithEmailAndPassword))]
    public async Task Login_PasswordDoesNotMatch_ReturnsNull(
        LoginRequestDto loginRequest,
        Mock<UserManager<ApplicationUser>> userManagerMock,
        AuthService service)
    {
        // Arrange
        userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((new ApplicationUser()));
        userManagerMock.Setup(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        // Act
        var tokenResponse = await service.LoginAsync(loginRequest);

        // Assert
        tokenResponse.Should().BeNull();
    }

    #endregion

    #region LogoutAsync

    [Theory, MemberData(nameof(LogoutCases))]
    public async Task Logout_ValidRefreshTokenProvided_LogsOutSuccessfully(
        string refreshToken,
        Mock<IRefreshTokenService> refreshTokenMock,
        AuthService service)
    {
        // Arrange
        refreshTokenMock.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns((string hash) => hash);
        refreshTokenMock.Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync(new RefreshToken());
        refreshTokenMock.Setup(x => x.RevokeRefreshTokenAsync(It.IsAny<RefreshToken>())).ReturnsAsync(true);

        // Act
        var loggedOut = await service.LogoutAsync(refreshToken);

        // Assert
        loggedOut.Should().BeTrue();
    }

    [Theory, MemberData(nameof(LogoutCases))]
    public async Task Logout_RefreshTokenEntityNotFound_ReturnsFalse(
        string refreshToken,
        Mock<IRefreshTokenService> refreshTokenMock,
        AuthService service)
    {
        // Arrange
        refreshTokenMock.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns((string hash) => hash);
        refreshTokenMock.Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);


        // Act
        var loggedOut = await service.LogoutAsync(refreshToken);

        // Assert
        loggedOut.Should().BeFalse();
    }

    [Theory, MemberData(nameof(LogoutCasesWithRevokedRefreshToken))]
    public async Task Logout_RefreshTokenIsAlreadyRevoked_ReturnsFalse(
        string refreshToken,
        RefreshToken storedRefreshToken,
        Mock<IRefreshTokenService> refreshTokenMock,
        AuthService service)
    {
        // Arrange
        storedRefreshToken.RevokedAt = new DateTime(2026, 1, 1);
        refreshTokenMock.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns((string hash) => hash);
        refreshTokenMock.Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync(storedRefreshToken);
        // Act
        var loggedOut = await service.LogoutAsync(refreshToken);

        // Assert
        loggedOut.Should().BeFalse();
    }

    [Theory, MemberData(nameof(LogoutCases))]
    public async Task Logout_CouldNotRevokeTheRefreshToken_ReturnsFalse(
        string refreshToken,
        Mock<IRefreshTokenService> refreshTokenMock,
        AuthService service)
    {
        // Arrange
        refreshTokenMock.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns((string hash) => hash);
        refreshTokenMock.Setup(x => x.GetRefreshTokenAsync(It.IsAny<string>())).ReturnsAsync(new RefreshToken());
        refreshTokenMock.Setup(x => x.RevokeRefreshTokenAsync(It.IsAny<RefreshToken>())).ReturnsAsync(false);
        // Act
        var loggedOut = await service.LogoutAsync(refreshToken);

        // Assert
        loggedOut.Should().BeFalse();
    }

    #endregion

    #region RefreshAsync

    [Theory, MemberData(nameof(RefreshCases))]
    public async Task Refresh_ValidRefreshToken_ReturnsNewTokenResponse(
        RefreshRequestDto refreshRequest,
        ApplicationUser user,
        ClaimsPrincipal principal,
        RefreshToken storedToken,
        JwtTokenDto jwtTokenDto,
        Mock<UserManager<ApplicationUser>> userManagerMock,
        Mock<IJwtService> jwtServiceMock,
        Mock<IRefreshTokenService> refreshTokenMock,
        FakeTimeProvider fakeTimeProvider,
        JwtOptions jwtOptions,
        AuthService service)
    {
        // Arrange
        const string newRefreshToken = "new-refresh-token";
        var roles = new List<string> { "User" };
        var frozenTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(frozenTime);
        var userId = Guid.NewGuid();
        storedToken.ApplicationUserId = userId;
        storedToken.RevokedAt = null;
        storedToken.ExpiresAt = frozenTime.UtcDateTime.AddDays(1);

        jwtServiceMock.Setup(x => x.GetPrincipleFromExpiredToken(refreshRequest.ExpiredAccessToken)).Returns(principal);
        refreshTokenMock.Setup(x => x.HashRefreshToken(refreshRequest.RefreshToken)).Returns("old-token-hash");
        refreshTokenMock.Setup(x => x.GetRefreshTokenWithUserIdAsync(It.IsAny<Guid>(), "old-token-hash"))
            .ReturnsAsync(storedToken);
        userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(roles);
        jwtServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<JwtUserDto>())).Returns(jwtTokenDto);
        refreshTokenMock.Setup(x => x.GenerateRefreshToken()).Returns(newRefreshToken);
        refreshTokenMock.Setup(x => x.HashRefreshToken(newRefreshToken)).Returns("new-token-hash");
        refreshTokenMock.Setup(x => x.RevokeRefreshTokenAsync(storedToken)).ReturnsAsync(true);
        refreshTokenMock.Setup(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);

        // Act
        var tokenResponse = await service.RefreshAsync(refreshRequest);

        // Assert
        tokenResponse.Should().NotBeNull();
        tokenResponse.AccessToken.Should().Be(jwtTokenDto.Token);
        tokenResponse.RefreshToken.Should().Be(newRefreshToken);
        tokenResponse.AccessTokenExpiresAt.Should().Be(jwtTokenDto.ExpiresAt);

        refreshTokenMock.Verify(x => x.RevokeRefreshTokenAsync(storedToken), Times.Once);
        refreshTokenMock.Verify(x => x.AddRefreshTokenAsync(
            It.Is<RefreshToken>(token =>
                token.ApplicationUserId == user.Id &&
                token.TokenHash == "new-token-hash" &&
                token.ExpiresAt == fakeTimeProvider.GetUtcNow().UtcDateTime
                    .AddDays(jwtOptions.RefreshTokenExpirationDays))), Times.Once);
    }

    [Theory, MemberData(nameof(RefreshCasesWithNoPrinciple))]
    public async Task Refresh_PrincipleIsNull_ReturnsNull(
        RefreshRequestDto refreshRequest,
        Mock<IJwtService> jwtServiceMock,
        AuthService service)
    {
        // Arrange
        jwtServiceMock.Setup(x => x.GetPrincipleFromExpiredToken(refreshRequest.ExpiredAccessToken))
            .Returns((ClaimsPrincipal?)null);

        // Act
        var tokenResponse = await service.RefreshAsync(refreshRequest);

        // Assert
        tokenResponse.Should().BeNull();
    }

    [Theory, MemberData(nameof(RefreshCasesWithNullNameIdentifier))]
    public async Task Refresh_UserClaimsNameIdentifierIsNull_ReturnsNull(
        ClaimsPrincipal principal,
        RefreshRequestDto refreshRequest,
        Mock<IJwtService> jwtServiceMock,
        AuthService service)
    {
        // Arrange
        jwtServiceMock.Setup(x => x.GetPrincipleFromExpiredToken(refreshRequest.ExpiredAccessToken)).Returns(principal);

        // Act
        var tokenResponse = await service.RefreshAsync(refreshRequest);

        // Assert
        tokenResponse.Should().BeNull();
    }

    [Theory, MemberData(nameof(RefreshCasesWithNonParsableNameIdentifier))]
    public async Task Refresh_UserClaimsNameIdentifierCannotBeParsed_ReturnsNull(
        ClaimsPrincipal principal,
        RefreshRequestDto refreshRequest,
        Mock<IJwtService> jwtServiceMock,
        AuthService service)
    {
        // Arrange
        jwtServiceMock.Setup(x => x.GetPrincipleFromExpiredToken(refreshRequest.ExpiredAccessToken)).Returns(principal);

        // Act
        var tokenResponse = await service.RefreshAsync(refreshRequest);

        // Assert
        tokenResponse.Should().BeNull();
    }

    [Theory, MemberData(nameof(RefreshCasesWithRevokedToken))]
    public async Task Refresh_RefreshTokenIsAlreadyRevoked_ReturnsNull(
        ClaimsPrincipal principal,
        RefreshToken storedToken,
        RefreshRequestDto refreshRequest,
        Mock<IJwtService> jwtServiceMock,
        Mock<IRefreshTokenService> refreshTokenMock,
        AuthService service)
    {
        // Arrange
        storedToken.RevokedAt = new DateTime(2026, 1, 1); // already revoked
        jwtServiceMock.Setup(x => x.GetPrincipleFromExpiredToken(refreshRequest.ExpiredAccessToken)).Returns(principal);
        refreshTokenMock.Setup(x => x.HashRefreshToken(refreshRequest.RefreshToken)).Returns("old-token-hash");
        refreshTokenMock.Setup(x => x.GetRefreshTokenWithUserIdAsync(It.IsAny<Guid>(), "old-token-hash"))
            .ReturnsAsync(storedToken);
        // Act
        var tokenResponse = await service.RefreshAsync(refreshRequest);

        // Assert
        tokenResponse.Should().BeNull();
    }

    [Theory, MemberData(nameof(RefreshCasesWithExpiredToken))]
    public async Task Refresh_RefreshTokenIsExpired_ReturnsNull(
        ClaimsPrincipal principal,
        RefreshToken storedToken,
        RefreshRequestDto refreshRequest,
        Mock<IJwtService> jwtServiceMock,
        Mock<IRefreshTokenService> refreshTokenMock,
        FakeTimeProvider fakeTimeProvider,
        AuthService service)
    {
        // Arrange
        var frozenTime = new DateTimeOffset(2026, 1, 7, 12, 0, 0, 0, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(frozenTime);
        storedToken.ExpiresAt = new DateTime(2026, 1, 2, 12, 0, 0); // already expired
        storedToken.RevokedAt = null;
        jwtServiceMock.Setup(x => x.GetPrincipleFromExpiredToken(refreshRequest.ExpiredAccessToken)).Returns(principal);
        refreshTokenMock.Setup(x => x.HashRefreshToken(refreshRequest.RefreshToken)).Returns("old-token-hash");
        refreshTokenMock.Setup(x => x.GetRefreshTokenWithUserIdAsync(It.IsAny<Guid>(), "old-token-hash"))
            .ReturnsAsync(storedToken);
        // Act
        var tokenResponse = await service.RefreshAsync(refreshRequest);

        // Assert
        tokenResponse.Should().BeNull();
    }

    [Theory, MemberData(nameof(RefreshCasesWithUserNotFound))]
    public async Task Refresh_UserAssociatedWithTokenNotFound_ReturnsNull(
        ClaimsPrincipal principal,
        RefreshToken storedToken,
        RefreshRequestDto refreshRequest,
        Mock<IJwtService> jwtServiceMock,
        Mock<IRefreshTokenService> refreshTokenMock,
        Mock<UserManager<ApplicationUser>> userManagerMock,
        AuthService service)
    {
        // Arrange
        storedToken.RevokedAt = null;
        jwtServiceMock.Setup(x => x.GetPrincipleFromExpiredToken(refreshRequest.ExpiredAccessToken)).Returns(principal);
        refreshTokenMock.Setup(x => x.HashRefreshToken(refreshRequest.RefreshToken)).Returns("old-token-hash");
        refreshTokenMock.Setup(x => x.GetRefreshTokenWithUserIdAsync(It.IsAny<Guid>(), "old-token-hash"))
            .ReturnsAsync(storedToken);
        refreshTokenMock.Setup(x => x.RevokeRefreshTokenAsync(storedToken)).ReturnsAsync(true);
        userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        // Act
        var tokenResponse = await service.RefreshAsync(refreshRequest);

        // Assert
        tokenResponse.Should().BeNull();
    }

    #endregion

    #region MemberData Cases

    public static IEnumerable<object[]> RegisterCases()
    {
        var fixtureConfigs = ConfigureFixtureWithIdentityAndBehaviours();
        var fixture = fixtureConfigs.Fixture;
        var userManagerMock = fixtureConfigs.UserManagerMock;

        var roleStore = new Mock<IRoleStore<ApplicationRole>>();

        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object, null!, null!, null!, null!);

        fixture.Inject(roleManagerMock);
        fixture.Inject(roleManagerMock.Object);

        var registerRequest = fixture.Create<RegisterRequestDto>();
        var service = fixture.Create<AuthService>();

        yield return new object[] { registerRequest, userManagerMock, roleManagerMock, service };
    }

    public static IEnumerable<object[]> RegisterCasesWithAccountCreationFailure()
    {
        var fixtureConfigs = ConfigureFixtureWithIdentityAndBehaviours();
        var fixture = fixtureConfigs.Fixture;
        var userManagerMock = fixtureConfigs.UserManagerMock;

        var registerRequest = fixture.Create<RegisterRequestDto>();
        var service = fixture.Create<AuthService>();

        yield return new object[] { registerRequest, userManagerMock, service };
    }

    public static IEnumerable<Object[]> RegisterCasesWithExistingIdentityUser()
    {
        var fixtureConfigs = ConfigureFixtureWithIdentityAndBehaviours();
        var fixture = fixtureConfigs.Fixture;
        var userManagerMock = fixtureConfigs.UserManagerMock;

        var registerRequest = fixture.Create<RegisterRequestDto>();
        var user = fixture.Create<ApplicationUser>();
        var service = fixture.Create<AuthService>();

        yield return new object[] { user, registerRequest, userManagerMock, service };
    }

    public static IEnumerable<object[]> LoginCases()
    {
        var fixtureConfigs = ConfigureFixtureWithIdentityAndBehaviours();
        var fixture = fixtureConfigs.Fixture;
        var userManagerMock = fixtureConfigs.UserManagerMock;

        var refreshTokenMock = new Mock<IRefreshTokenService>();
        fixture.Inject(refreshTokenMock.Object);

        var jwtServiceMock = new Mock<IJwtService>();
        fixture.Inject(jwtServiceMock.Object);

        var loginRequest = fixture.Create<LoginRequestDto>();
        var jwtTokenDto = fixture.Create<JwtTokenDto>();
        var service = fixture.Create<AuthService>();

        yield return new object[]
        {
            loginRequest, jwtTokenDto, jwtServiceMock, refreshTokenMock, userManagerMock, service
        };
    }

    public static IEnumerable<object[]> LoginCasesWithEmailAndPassword()
    {
        var fixtureConfigs = ConfigureFixtureWithIdentityAndBehaviours();
        var fixture = fixtureConfigs.Fixture;
        var userManagerMock = fixtureConfigs.UserManagerMock;

        var loginRequest = fixture.Create<LoginRequestDto>();
        var service = fixture.Create<AuthService>();

        yield return new object[] { loginRequest, userManagerMock, service };
    }

    public static IEnumerable<object[]> LogoutCases()
    {
        var fixtureConfigs = ConfigureFixtureWithIdentityAndBehaviours();
        var fixture = fixtureConfigs.Fixture;

        var refreshTokenMock = new Mock<IRefreshTokenService>();
        fixture.Inject(refreshTokenMock.Object);

        var refreshToken = fixture.Create<string>();
        var service = fixture.Create<AuthService>();

        yield return new object[] { refreshToken, refreshTokenMock, service };
    }

    public static IEnumerable<object[]> LogoutCasesWithRevokedRefreshToken()
    {
        var fixtureConfigs = ConfigureFixtureWithIdentityAndBehaviours();
        var fixture = fixtureConfigs.Fixture;

        var refreshTokenMock = new Mock<IRefreshTokenService>();
        fixture.Inject(refreshTokenMock.Object);

        var refreshToken = fixture.Create<string>();
        var storedRefreshToken = fixture.Create<RefreshToken>();
        var service = fixture.Create<AuthService>();

        yield return new object[] { refreshToken, storedRefreshToken, refreshTokenMock, service };
    }


    public static IEnumerable<object[]> RefreshCases()
    {
        var fixtureConfigs = ConfigureFixtureWithIdentityAndBehaviours();
        var fixture = fixtureConfigs.Fixture;
        var userManagerMock = fixtureConfigs.UserManagerMock;

        var refreshTokenMock = new Mock<IRefreshTokenService>();
        fixture.Inject(refreshTokenMock.Object);

        var jwtServiceMock = new Mock<IJwtService>();
        fixture.Inject(jwtServiceMock.Object);

        var jwtOptions = fixture.Create<JwtOptions>();
        fixture.Inject(jwtOptions);
        fixture.Inject(Options.Create(jwtOptions));

        var fakeTimeProvider = new FakeTimeProvider();
        fixture.Inject<TimeProvider>(fakeTimeProvider);

        var refreshRequest = fixture.Create<RefreshRequestDto>();
        var jwtTokenDto = fixture.Create<JwtTokenDto>();
        var user = fixture.Create<ApplicationUser>();

        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        var storedToken = fixture.Create<RefreshToken>();
        var service = fixture.Create<AuthService>();

        yield return new object[]
        {
            refreshRequest, user, principal, storedToken, jwtTokenDto, userManagerMock, jwtServiceMock,
            refreshTokenMock, fakeTimeProvider, jwtOptions, service
        };
    }

    public static IEnumerable<object[]> RefreshCasesWithNoPrinciple()
    {
        var fixtureConfigs = ConfigureFixtureWithIdentityAndBehaviours();
        var fixture = fixtureConfigs.Fixture;

        var jwtServiceMock = new Mock<IJwtService>();
        fixture.Inject(jwtServiceMock.Object);

        var refreshRequest = fixture.Create<RefreshRequestDto>();
        var service = fixture.Create<AuthService>();

        yield return new object[] { refreshRequest, jwtServiceMock, service };
    }

    public static IEnumerable<object[]> RefreshCasesWithNullNameIdentifier()
    {
        var fixtureConfigs = ConfigureFixtureWithIdentityAndBehaviours();
        var fixture = fixtureConfigs.Fixture;

        var jwtServiceMock = new Mock<IJwtService>();
        fixture.Inject(jwtServiceMock.Object);

        var claims = new[] { new Claim(ClaimTypes.Name, "TEST") }; // random claim
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        var refreshRequest = fixture.Create<RefreshRequestDto>();
        var service = fixture.Create<AuthService>();

        yield return new object[] { principal, refreshRequest, jwtServiceMock, service };
    }

    public static IEnumerable<object[]> RefreshCasesWithNonParsableNameIdentifier()
    {
        var fixtureConfigs = ConfigureFixtureWithIdentityAndBehaviours();
        var fixture = fixtureConfigs.Fixture;

        var jwtServiceMock = new Mock<IJwtService>();
        fixture.Inject(jwtServiceMock.Object);

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "NonParsableId") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        var refreshRequest = fixture.Create<RefreshRequestDto>();
        var service = fixture.Create<AuthService>();

        yield return new object[] { principal, refreshRequest, jwtServiceMock, service };
    }

    public static IEnumerable<object[]> RefreshCasesWithRevokedToken()
    {
        var fixtureConfigs = ConfigureFixtureWithIdentityAndBehaviours();
        var fixture = fixtureConfigs.Fixture;

        var jwtServiceMock = new Mock<IJwtService>();
        fixture.Inject(jwtServiceMock.Object);

        var refreshTokenMock = new Mock<IRefreshTokenService>();
        fixture.Inject(refreshTokenMock.Object);

        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        var storedToken = fixture.Create<RefreshToken>();
        var refreshRequest = fixture.Create<RefreshRequestDto>();
        var service = fixture.Create<AuthService>();

        yield return new object[] { principal, storedToken, refreshRequest, jwtServiceMock, refreshTokenMock, service };
    }

    public static IEnumerable<object[]> RefreshCasesWithExpiredToken()
    {
        var fixtureConfigs = ConfigureFixtureWithIdentityAndBehaviours();
        var fixture = fixtureConfigs.Fixture;

        var jwtServiceMock = new Mock<IJwtService>();
        fixture.Inject(jwtServiceMock.Object);

        var refreshTokenMock = new Mock<IRefreshTokenService>();
        fixture.Inject(refreshTokenMock.Object);

        var fakeTimeProvider = new FakeTimeProvider();
        fixture.Inject<TimeProvider>(fakeTimeProvider);

        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        var storedToken = fixture.Create<RefreshToken>();
        var refreshRequest = fixture.Create<RefreshRequestDto>();
        var service = fixture.Create<AuthService>();

        yield return new object[]
        {
            principal, storedToken, refreshRequest, jwtServiceMock, refreshTokenMock, fakeTimeProvider, service
        };
    }

    public static IEnumerable<object[]> RefreshCasesWithUserNotFound()
    {
        var fixtureConfigs = ConfigureFixtureWithIdentityAndBehaviours();
        var fixture = fixtureConfigs.Fixture;
        var userManagerMock = fixtureConfigs.UserManagerMock;

        var jwtServiceMock = new Mock<IJwtService>();
        fixture.Inject(jwtServiceMock.Object);

        var refreshTokenMock = new Mock<IRefreshTokenService>();
        fixture.Inject(refreshTokenMock.Object);

        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

        var storedToken = fixture.Create<RefreshToken>();
        var refreshRequest = fixture.Create<RefreshRequestDto>();
        var service = fixture.Create<AuthService>();

        yield return new object[]
        {
            principal, storedToken, refreshRequest, jwtServiceMock, refreshTokenMock, userManagerMock, service
        };
    }

    #endregion

    private static FixtureConfigs ConfigureFixtureWithIdentityAndBehaviours()
    {
        var fixture = new Fixture();
        var services = new ServiceCollection();
        services.AddMetrics();
        var provider = services.BuildServiceProvider();
        fixture.Inject<IServiceProvider>(provider);

        fixture.Customize(new AutoMoqCustomization());
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var store = new Mock<IUserEmailStore<ApplicationUser>>();
        store.As<IUserRoleStore<ApplicationUser>>();

        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        fixture.Inject(userManagerMock.Object);

        return new FixtureConfigs { Fixture = fixture, UserManagerMock = userManagerMock };
    }

    private class FixtureConfigs
    {
        public Fixture Fixture { get; init; } = null!;
        public Mock<UserManager<ApplicationUser>> UserManagerMock { get; init; } = null!;
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using AutoFixture;
using AutoFixture.Xunit2;

using FluentAssertions;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;

using ShipmentManagement.Application.DTOs.Auth;
using ShipmentManagement.Infrastructure.Services;
using ShipmentManagement.Testing.Common;

namespace ShipmentManagement.Infrastructure.Tests.Services;

public class JwtServiceTests
{
    #region GenerateAccessToken

    [Theory, AutoMoqData]
    public void GenerateAccessToken_ValidUserProvided_ReturnsValidJwtToken(
        IFixture fixture,
        [Frozen] JwtOptions jwtOptions,
        [Frozen] JwtUserDto user,
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider timeProvider)
    {
        // Arrange
        var frozenTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        timeProvider.SetUtcNow(frozenTime);

        jwtOptions.Key = "super_secret_key_that_is_long_enough_32_chars";
        jwtOptions.AccessTokenExpirationMinutes = 30;

        fixture.Inject(Options.Create(jwtOptions));
        var service = fixture.Create<JwtService>();

        // Act
        var result = service.GenerateAccessToken(user);

        // Assert
        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().Be(frozenTime.UtcDateTime.AddMinutes(jwtOptions.AccessTokenExpirationMinutes));
    }

    [Theory, AutoMoqData]
    public void GenerateAccessToken_ValidUserProvided_PopulatesExpectedClaimsAndMetadata(
        IFixture fixture,
        [Frozen] JwtOptions jwtOptions,
        [Frozen] JwtUserDto user,
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider timeProvider)
    {
        // Arrange
        var frozenTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        timeProvider.SetUtcNow(frozenTime);

        jwtOptions.Key = "super_secret_key_that_is_long_enough_32_chars";
        jwtOptions.AccessTokenExpirationMinutes = 30;
        jwtOptions.Issuer = "TestIssuer";
        jwtOptions.Audience = "TestAudience";

        fixture.Inject(Options.Create(jwtOptions));
        var service = fixture.Create<JwtService>();

        // Act
        var result = service.GenerateAccessToken(user);
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(result.Token);

        // Assert
        jwtToken.Header.Alg.Should().Be("HS256");
        jwtToken.Issuer.Should().Be(jwtOptions.Issuer);
        jwtToken.Audiences.Should().Contain(a => a == jwtOptions.Audience);

        var nameIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        var iatClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat);
        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        nameIdClaim.Should().NotBeNull();
        nameIdClaim.Value.Should().Be(user.Id.ToString());
        emailClaim.Should().NotBeNull();
        emailClaim.Value.Should().Be(user.Email);
        jtiClaim.Should().NotBeNull();
        jtiClaim.Value.Should().NotBeNullOrEmpty();
        iatClaim.Should().NotBeNull();
        iatClaim.Value.Should().Be(frozenTime.ToUnixTimeSeconds().ToString());
        roleClaims.Should().Contain(user.Roles);
    }

    [Theory, AutoMoqData]
    public void GenerateAccessToken_ValidTokenValidationParams_ProducesCryptographicallyValidToken(
        IFixture fixture,
        [Frozen] JwtOptions jwtOptions,
        [Frozen] JwtUserDto user)
    {
        // Arrange

        jwtOptions.Key = "super_secret_key_that_is_long_enough_32_chars";
        jwtOptions.Issuer = "TestIssuer";
        jwtOptions.Audience = "TestAudience";

        fixture.Inject(Options.Create(jwtOptions));
        var service = fixture.Create<JwtService>();

        // Act
        var result = service.GenerateAccessToken(user);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // can be ignored for basic signature checking
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
        };

        Action validateTokenAction = () => tokenHandler.ValidateToken(result.Token, validationParameters, out _);
        validateTokenAction.Should().NotThrow("Because the token should be valid and correctly signed");
    }

    [Theory, AutoMoqData]
    public void GenerateAccessToken_EmptyUserIdentifiers_StillGeneratesValidToken(
        IFixture fixture,
        [Frozen] JwtOptions jwtOptions)
    {
        // Arrange
        var user = new JwtUserDto
        {
            Id = Guid.Empty, Name = string.Empty, Email = string.Empty, Roles = new List<string>()
        };

        jwtOptions.Key = "super_secret_key_that_is_long_enough_32_chars";
        fixture.Inject(Options.Create(jwtOptions));
        var service = fixture.Create<JwtService>();

        // Act
        var result = service.GenerateAccessToken(user);

        // Assert
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Theory, AutoMoqData]
    public void GenerateAccessToken_MinimalExpirationTime_SetsCorrectExpirationBounds(
        IFixture fixture,
        [Frozen] JwtOptions jwtOptions,
        [Frozen] JwtUserDto user,
        [Frozen(Matching.DirectBaseType)] FakeTimeProvider timeProvider)
    {
        // Arrange
        var frozenTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        timeProvider.SetUtcNow(frozenTime);

        jwtOptions.Key = "super_secret_key_that_is_long_enough_32_chars";
        jwtOptions.AccessTokenExpirationMinutes = 1; // 1-minute expiration for testing edge case

        fixture.Inject(Options.Create(jwtOptions));
        var service = fixture.Create<JwtService>();

        // Act
        var result = service.GenerateAccessToken(user);

        // Assert
        result.Token.Should().NotBeNullOrEmpty();
        result.ExpiresAt.Should().Be(frozenTime.UtcDateTime.AddMinutes(1));
    }

    #endregion

    #region GetPrincipleFromExpiredToken

    [Theory, AutoMoqData]
    public void GetPrincipleFromExpiredToken_ValidExpiredToken_ReturnsClaimsPrincipal(
        IFixture fixture,
        [Frozen] JwtOptions jwtOptions,
        [Frozen] JwtUserDto user)
    {
        // Arrange
        jwtOptions.Key = "super_secret_key_that_is_long_enough_32_chars";
        fixture.Inject(Options.Create(jwtOptions));
        var service = fixture.Create<JwtService>();

        var expiredToken = service.GenerateAccessToken(user).Token;

        // Act
        var principal = service.GetPrincipleFromExpiredToken(expiredToken);

        // Assert
        principal.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Theory, AutoMoqData]
    public void GetPrincipleFromExpiredToken_InvalidToken_ReturnsNull(
        string invalidToken,
        IFixture fixture,
        [Frozen] JwtOptions jwtOptions)
    {
        // Arrange
        fixture.Inject(Options.Create(jwtOptions));
        var service = fixture.Create<JwtService>();

        // Act
        var principal = service.GetPrincipleFromExpiredToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public void GetPrincipleFromExpiredToken_ValidExpiredTokenWithInvalidSignature_ReturnsNull(
        IFixture fixture,
        [Frozen] JwtOptions jwtOptions,
        [Frozen] JwtUserDto user)
    {
        // Arrange
        jwtOptions.Key = "super_secret_key_that_is_long_enough_32_chars";
        fixture.Inject(Options.Create(jwtOptions));
        var service = fixture.Create<JwtService>();

        var expiredToken = service.GenerateAccessToken(user).Token;

        var tamperedToken = expiredToken + "tampered";

        // Act
        var principal = service.GetPrincipleFromExpiredToken(tamperedToken);

        // Assert
        principal.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public void GetPrincipleFromExpiredToken_ValidExpiredTokenWithDifferentIssuer_ReturnsNull(
        IFixture fixture,
        [Frozen] JwtOptions jwtOptions,
        [Frozen] JwtUserDto user)
    {
        // Arrange
        jwtOptions.Key = "super_secret_key_that_is_long_enough_32_chars";
        fixture.Inject(Options.Create(jwtOptions));
        var service = fixture.Create<JwtService>();

        var expiredToken = service.GenerateAccessToken(user).Token;

        jwtOptions.Issuer = "DifferentIssuer";

        // Act
        var principal = service.GetPrincipleFromExpiredToken(expiredToken);

        // Assert
        principal.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public void GetPrincipleFromExpiredToken_ValidExpiredTokenWithDifferentAudience_ReturnsNull(
        IFixture fixture,
        [Frozen] JwtOptions jwtOptions,
        [Frozen] JwtUserDto user)
    {
        // Arrange
        jwtOptions.Key = "super_secret_key_that_is_long_enough_32_chars";
        fixture.Inject(Options.Create(jwtOptions));
        var service = fixture.Create<JwtService>();

        var expiredToken = service.GenerateAccessToken(user).Token;

        jwtOptions.Audience = "DifferentAudience";

        // Act
        var principal = service.GetPrincipleFromExpiredToken(expiredToken);

        // Assert
        principal.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public void GetPrincipleFromExpiredToken_ValidExpiredTokenWithDifferentKey_ReturnsNull(
        IFixture fixture,
        [Frozen] JwtOptions jwtOptions,
        [Frozen] JwtUserDto user)
    {
        // Arrange
        jwtOptions.Key = "super_secret_key_that_is_long_enough_32_chars";
        fixture.Inject(Options.Create(jwtOptions));
        var service = fixture.Create<JwtService>();

        var expiredToken = service.GenerateAccessToken(user).Token;

        jwtOptions.Key = "different_super_secret_key_that_is_long_enough_32_chars";

        // Act
        var principal = service.GetPrincipleFromExpiredToken(expiredToken);

        // Assert
        principal.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public void GetPrincipleFromExpiredToken_ValidExpiredTokenWithTamperedClaims_ReturnsNull(
        IFixture fixture,
        [Frozen] JwtOptions jwtOptions,
        [Frozen] JwtUserDto user)
    {
        // Arrange
        jwtOptions.Key = "super_secret_key_that_is_long_enough_32_chars";
        fixture.Inject(Options.Create(jwtOptions));
        var service = fixture.Create<JwtService>();

        var expiredToken = service.GenerateAccessToken(user).Token;

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(expiredToken);
        var tamperedClaims = jwtToken.Claims.ToList();
        tamperedClaims.Add(new Claim("tampered", "true"));

        var tamperedJwtToken = new JwtSecurityToken(
            jwtToken.Issuer,
            jwtToken.Audiences.FirstOrDefault(),
            tamperedClaims,
            jwtToken.ValidFrom,
            jwtToken.ValidTo,
            jwtToken.SigningCredentials
        );

        var tamperedTokenString = tokenHandler.WriteToken(tamperedJwtToken);

        // Act
        var principal = service.GetPrincipleFromExpiredToken(tamperedTokenString);

        // Assert
        principal.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public void GetPrincipleFromExpiredToken_ExpiredTokenWithDifferentSecurityAlgorithm_ReturnsNull(
        IFixture fixture,
        [Frozen] JwtOptions jwtOptions,
        [Frozen] JwtUserDto user)
    {
        // Arrange
        jwtOptions.Key = new string('a', 64);  // Ensure the key is long enough for HmacSha512
        fixture.Inject(Options.Create(jwtOptions));
        var service = fixture.Create<JwtService>();

        var expiredToken = service.GenerateAccessToken(user).Token;
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(expiredToken);

        var jwtSecurityToken = new JwtSecurityToken(
            jwtToken.Issuer,
            jwtToken.Audiences.FirstOrDefault(),
            jwtToken.Claims.ToList(),
            jwtToken.ValidFrom,
            jwtToken.ValidTo,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                SecurityAlgorithms.HmacSha512) // using a different algorithm
        );
        
        var tamperedTokenString = tokenHandler.WriteToken(jwtSecurityToken);

        // Act
        var principal = service.GetPrincipleFromExpiredToken(tamperedTokenString);

        // Assert
        principal.Should().BeNull();
    }
    
    #endregion
}
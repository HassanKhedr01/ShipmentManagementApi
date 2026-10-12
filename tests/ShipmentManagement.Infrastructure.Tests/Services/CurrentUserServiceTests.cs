using System.Security.Claims;

using AutoFixture.Xunit2;

using ShipmentManagement.Infrastructure.Services;
using ShipmentManagement.Testing.Common;

using FluentAssertions;

using Microsoft.AspNetCore.Http;

using Moq;

namespace ShipmentManagement.Infrastructure.Tests.Services;

public class CurrentUserServiceTests
{
    #region UserId

    [Theory, AutoMoqData]
    public void UserId_ValidUserClaim_ReturnsParsedGuid(
        Guid expectedUserId,
        [Frozen] Mock<IHttpContextAccessor> mockAccessor,
        CurrentUserService service)
    {
        // Arrange
        SetupMockUser(mockAccessor, new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()));

        // Act
        var result = service.UserId;

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Theory, AutoMoqData]
    public void UserId_NonActiveHttpContext_ReturnsNull(
        [Frozen] Mock<IHttpContextAccessor> mockAccessor,
        CurrentUserService service)
    {
        // Arrange
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = service.UserId;

        // Assert
        result.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public void UserId_NullClaim_ReturnsNull(
        [Frozen] Mock<IHttpContextAccessor> mockAccessor,
        CurrentUserService service)
    {
        // Arrange
        SetupMockUser(mockAccessor);

        // Act
        var result = service.UserId;

        // Assert
        result.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public void UserId_FailToParseUserId_ReturnsNull(
        string invalidUserId,
        [Frozen] Mock<IHttpContextAccessor> mockAccessor,
        CurrentUserService service)
    {
        // Arrange
        SetupMockUser(mockAccessor, new Claim(ClaimTypes.NameIdentifier, invalidUserId));
        // Act
        var result = service.UserId;

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region IsAuthenticated

    [Theory, AutoMoqData]
    public void IsAuthenticated_UserIsAuthenticated_ReturnsTrue(
        [Frozen] Mock<IHttpContextAccessor> mockAccessor,
        CurrentUserService service)
    {
        // Arrange
        SetupMockUserIdentity(mockAccessor, true);

        // Act
        var result = service.IsAuthenticated;

        // Assert
        result.Should().BeTrue();
    }

    [Theory, AutoMoqData]
    public void IsAuthenticated_NonActiveHttpContext_ReturnsFalse(
        [Frozen] Mock<IHttpContextAccessor> mockAccessor,
        CurrentUserService service)
    {
        // Arrange
        mockAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = service.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    [Theory, AutoMoqData]
    public void IsAuthenticated_NullIdentity_ReturnsFalse(
        [Frozen] Mock<ClaimsPrincipal> mockPrincipal,
        [Frozen] Mock<HttpContext> mockContext,
        [Frozen] Mock<IHttpContextAccessor> mockAccessor,
        CurrentUserService service)
    {
        // Arrange
        mockPrincipal.Setup(x => x.Identity).Returns((ClaimsIdentity?)null);
        mockContext.Setup(x => x.User).Returns(mockPrincipal.Object);
        mockAccessor.Setup(x => x.HttpContext).Returns(mockContext.Object);
        
        // Act
        var result = service.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    [Theory, AutoMoqData]
    public void IsAuthenticated_UserNotAuthenticated_ReturnsFalse(
        [Frozen] Mock<IHttpContextAccessor> mockAccessor,
        CurrentUserService service)
    {
        // Arrange
        SetupMockUserIdentity(mockAccessor, false);

        // Act
        var result = service.IsAuthenticated;

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    private static void SetupMockUser(Mock<IHttpContextAccessor> mockAccessor, params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(x => x.User).Returns(principal);
        mockAccessor.Setup(x => x.HttpContext).Returns(mockContext.Object);
    }

    private static void SetupMockUserIdentity(Mock<IHttpContextAccessor> mockAccessor, bool isAuthenticated)
    {
        var mockIdentity = new Mock<ClaimsIdentity>();
        mockIdentity.Setup(x => x.IsAuthenticated).Returns(isAuthenticated);
        var mockPrincipal = new Mock<ClaimsPrincipal>();
        mockPrincipal.Setup(x => x.Identity).Returns(mockIdentity.Object);
        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(x => x.User).Returns(mockPrincipal.Object);
        mockAccessor.Setup(x => x.HttpContext).Returns(mockContext.Object);
    }
}
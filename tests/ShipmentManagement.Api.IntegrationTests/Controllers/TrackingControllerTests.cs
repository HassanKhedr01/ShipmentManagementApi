using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using ShipmentManagement.Api.IntegrationTests.Common;
using ShipmentManagement.Api.IntegrationTests.TestHost;
using ShipmentManagement.Api.Models;

namespace ShipmentManagement.Api.IntegrationTests.Controllers;

public class TrackingControllerTests
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerOptions.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    #region GetPackage

    [Fact]
    public async Task GetPackage_WithUnknownTrackingNumber_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("api/v1.0/tracking/PKG-00000000"); // unknown tracking number

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        var message = messageResponse?.Message;
        message.Should().Be("Package not found");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("PKG-00")] // < 12
    [InlineData("PKG-BIGNUMBER000000000")] // > 12
    public async Task GetPackage_WithInvalidTrackingNumber_ReturnsBadRequest(string? trackingNumber)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"api/v1.0/tracking/{trackingNumber}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        var message = messageResponse?.Message;
        message.Should().NotBeNull();
        message.Should().Be("Enter a valid tracking number");
    }

    [Theory]
    [InlineData("PKG-4A6B7C9D")]
    [InlineData("PKG-3E2F1G4H")]
    [InlineData("PKG-9G8H7I6J")]
    [InlineData("PKG-5K4L3M2N")]
    [InlineData("PKG-1O2P3Q4R")]
    [InlineData("PKG-5S6T7U8V")]
    public async Task GetPackage_ValidTrackingNumber_ReturnsOkWithThePackageInfo(string trackingNumber)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"api/v1.0/tracking/{trackingNumber}");
        var action = () => response.EnsureSuccessStatusCode();

        // Assert
        action.Should().NotThrow();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var packageInfo = await response.Content.ReadFromJsonAsync<PublicPackageInfo>(_jsonOptions);
        packageInfo.Should().NotBeNull();
        packageInfo.TrackingNumber.Should().Be(trackingNumber);
    }

    #endregion

    #region GetEvents

    [Fact]
    public async Task GetEvents_WithUnknownTrackingNumber_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("api/v1.0/tracking/PKG-00000000/events"); // unknown tracking number

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        var message = messageResponse?.Message;
        message.Should().Be("No events found for this package");
    }
    
    [Theory]
    [InlineData("PKG-00")] 
    [InlineData("PKG-BIGNUMBER000000000")] 
    public async Task GetEvents_WithInvalidTrackingNumber_ReturnsBadRequest(string? trackingNumber)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"api/v1.0/tracking/{trackingNumber}/events");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        var message = messageResponse?.Message;
        message.Should().NotBeNull();
        message.Should().Be("Enter a valid tracking number");
    }

    [Theory]
    [InlineData("PKG-4A6B7C9D")]
    [InlineData("PKG-3E2F1G4H")]
    [InlineData("PKG-9G8H7I6J")]
    [InlineData("PKG-5K4L3M2N")]
    [InlineData("PKG-1O2P3Q4R")]
    [InlineData("PKG-5S6T7U8V")]
    public async Task GetEvents_ValidTrackingNumber_ReturnsOkWithThePackageEvents(string trackingNumber)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"api/v1.0/tracking/{trackingNumber}/events");
        var action = () => response.EnsureSuccessStatusCode();

        // Assert
        action.Should().NotThrow();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var packageEvents = await response.Content.ReadFromJsonAsync<List<PublicTrackingEvent>>(_jsonOptions);
        packageEvents.Should().NotBeNull();
        packageEvents.Should().NotBeEmpty();
    }

    #endregion
}
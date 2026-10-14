using System.Net;

using ShipmentManagement.Api.IntegrationTests.TestHost;

namespace ShipmentManagement.Api.IntegrationTests.Controllers;

public class TrackingControllerTests
{
    [Fact]
    public async Task GetTracking_WithUnknownTrackingNumber_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("api/v1.0/Tracking/PKG-00000000"); // unknown tracking number
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
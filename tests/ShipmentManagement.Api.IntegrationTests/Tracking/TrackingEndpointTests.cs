using System.Net;

using ShipmentManagement.Api.IntegrationTests.TestHost;

namespace ShipmentManagement.Api.IntegrationTests.Tracking;

public class TrackingEndpointTests
{
    [Fact]
    public async Task GetTracking_WithUnknownTrackingNumber_ReturnsNotFound()
    {
        // Arrange
        var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        
        // Act
        var response = await client.GetAsync("api/v1.0/Tracking/PKG-123456AB"); // unknown tracking number
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
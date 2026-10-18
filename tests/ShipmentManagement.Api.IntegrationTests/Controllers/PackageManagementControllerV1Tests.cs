using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using FluentValidation;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ShipmentManagement.Api.IntegrationTests.Common;
using ShipmentManagement.Api.IntegrationTests.Helper;
using ShipmentManagement.Api.IntegrationTests.TestHost;
using ShipmentManagement.Application.DTOs.Common;
using ShipmentManagement.Application.DTOs.DeliveryAttempts;
using ShipmentManagement.Application.DTOs.Packages;
using ShipmentManagement.Application.DTOs.TrackingEvents;
using ShipmentManagement.Infrastructure.Persistence;

namespace ShipmentManagement.Api.IntegrationTests.Controllers;

public class PackageManagementControllerV1Tests
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    #region PackageManagementRequests

    [Theory]
    [InlineData("GET", "api/v1.0/admin/packages")]
    [InlineData("GET", "api/v1.0/admin/packages/1")]
    [InlineData("GET", "api/v1.0/admin/packages/1/events")]
    [InlineData("POST", "api/v1.0/admin/packages/1/events")]
    [InlineData("GET", "api/v1.0/admin/packages/1/delivery-attempts")]
    public async Task PackageManagementRequest_NotAuthenticated_ReturnsUnauthorized(string method, string url)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), url);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("GET", "api/v1.0/admin/packages")]
    [InlineData("GET", "api/v1.0/admin/packages/1")]
    [InlineData("GET", "api/v1.0/admin/packages/1/events")]
    [InlineData("POST", "api/v1.0/admin/packages/1/events")]
    [InlineData("GET", "api/v1.0/admin/packages/1/delivery-attempts")]
    public async Task PackageManagementRequest_NotAdmin_ReturnsForbidden(string method, string url)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);
        using var request = new HttpRequestMessage(new HttpMethod(method), url);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region PackageRetrieval

    [Fact]
    public async Task ListPackages_AsAdmin_ReturnsAllPackages()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);

        // Act
        var response = await client.GetAsync("api/v1.0/admin/packages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var packages = await response.Content.ReadFromJsonAsync<List<PackageV1Dto>>(_jsonOptions);
        packages.Should().NotBeNull();
        packages.Should().HaveCount(7);
    }

    [Fact]
    public async Task ListPackages_NoPackagesExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await context.TrackingEvents.ExecuteDeleteAsync();
        await context.DeliveryAttempts.ExecuteDeleteAsync();
        await context.Packages.ExecuteDeleteAsync();
        
        // Act
        var response = await client.GetAsync("api/v1.0/admin/packages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public async Task GetPackage_PackageExists_ReturnsPackage(int id)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);

        // Act
        var response = await client.GetAsync($"api/v1.0/admin/packages/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var package = await response.Content.ReadFromJsonAsync<PackageV1Dto>(_jsonOptions);
        package.Should().NotBeNull();
        package.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetPackage_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        const int packageId = int.MaxValue;
        // Act
        var response = await client.GetAsync($"api/v1.0/admin/packages/{packageId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region TrackingEventRetrieval

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 1)]
    [InlineData(5, 2)]
    [InlineData(7, 7)]
    public async Task TrackPackage_ExistingPackageWithEvents_ReturnsEvents(int packageId, int eventCount)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);

        // Act
        var response = await client.GetAsync($"api/v1.0/admin/packages/{packageId}/events");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var events = await response.Content.ReadFromJsonAsync<List<TrackingEventDto>>(_jsonOptions);
        events.Should().NotBeNull();
        events.Should().HaveCount(eventCount);
    }

    [Fact]
    public async Task TrackPackage_PackageWithoutEvents_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        const int packageId = int.MaxValue;

        // Act
        var response =
            await client.GetAsync($"api/v1.0/admin/packages/{packageId}/events"); // no events for non-existing package

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region RecordTrackingEventSuccess

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task RecordEvent_ValidTransition_ReturnsEventAndUpdatesPackage(int packageId)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var request = new CreateTrackingEventDto
        {
            NewStatus = Status.InTransit, Description = "Package is in transit", FacilityId = 1
        };

        // Act
        var response = await client.PostAsJsonAsync($"api/v1.0/admin/packages/{packageId}/events", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var trackingEvent = await response.Content.ReadFromJsonAsync<TrackingEventDto>(_jsonOptions);
        trackingEvent.Should().NotBeNull();
        trackingEvent.Status.Should().Be(Status.InTransit);
        trackingEvent.Description.Should().Be(request.Description);
        var updatedPackage = await context.Packages.FirstOrDefaultAsync(x => x.Id == packageId);
        updatedPackage.Should().NotBeNull();
        updatedPackage.CurrentStatus.Should().Be(Domain.Enums.Status.InTransit);
        var savedEvent = await context.TrackingEvents
            .FirstOrDefaultAsync(e => e.PackageId == packageId && e.Status == Domain.Enums.Status.InTransit);
        savedEvent.Should().NotBeNull();
        savedEvent.Description.Should().Be(request.Description);
        savedEvent.FacilityId.Should().Be(1);
    }

    [Fact]
    public async Task RecordEvent_DeliveredStatus_RecordsSuccessfulDeliveryAttempt()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var request = new CreateTrackingEventDto
        {
            NewStatus = Status.Delivered, Description = "Package delivered successfully"
        };
        const int packageId = 6; // PackageId of the package to be delivered

        // Act
        var response = await client.PostAsJsonAsync($"api/v1.0/admin/packages/{packageId}/events", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedPackage = await context.Packages.FirstOrDefaultAsync(p => p.Id == packageId);
        updatedPackage.Should().NotBeNull();
        updatedPackage.CurrentStatus.Should().Be(Domain.Enums.Status.Delivered);
        var attempt = await context.DeliveryAttempts
            .FirstOrDefaultAsync(a => a.PackageId == packageId && a.DeliveryResult == Domain.Enums.Result.Successful);
        attempt.Should().NotBeNull();
    }

    #endregion

    #region RecordTrackingEventFailures

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public async Task RecordEvent_InvalidCreateRequest_ReturnsBadRequest(int id)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        var request = new CreateTrackingEventDto { NewStatus = (Status)999, Description = new string('D', 257) };
        var validator = factory.Services.GetRequiredService<IValidator<CreateTrackingEventDto>>();
        var validationResults = await validator.ValidateAsync(request);

        // Act
        var response = await client.PostAsJsonAsync($"api/v1.0/admin/packages/{id}/events", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errors = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().BeEquivalentTo(validationResults.ToDictionary());
    }

    [Fact]
    public async Task RecordEvent_NonExistingPackage_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        var request = new CreateTrackingEventDto { NewStatus = Status.PickedUp, Description = "Package picked up" };
        const int packageId = int.MaxValue;

        // Act
        var response = await client.PostAsJsonAsync($"api/v1.0/admin/packages/{packageId}/events", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(7)]
    public async Task RecordEvent_TransitionNotAllowed_ReturnsBadRequest(int id)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);

        var request = new CreateTrackingEventDto { NewStatus = Status.Delivered, Description = "Delivered" };

        // Act
        var response = await client.PostAsJsonAsync($"api/v1.0/admin/packages/{id}/events", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task RecordEvent_InactiveFacility_ReturnsBadRequest(int packageId)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);

        var request = new CreateTrackingEventDto
        {
            NewStatus = Status.InTransit, Description = "Package in transit", FacilityId = 5 // not active
        };

        // Act
        var response = await client.PostAsJsonAsync($"api/v1.0/admin/packages/{packageId}/events", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region DeliveryAttemptRetrieval

    [Fact]
    public async Task ListDeliveryAttempts_PackageWithAttempts_ReturnsAttempts()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        const int packageId = 7;

        // Act
        var response = await client.GetAsync($"api/v1.0/admin/packages/{packageId}/delivery-attempts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var attempts = await response.Content.ReadFromJsonAsync<List<DeliveryAttemptDto>>(_jsonOptions);
        attempts.Should().NotBeNull();
        attempts.Should().HaveCount(2);
        attempts.Should().Contain(a => a.DeliveryResult == Result.Failed);
        attempts.Should().Contain(a => a.DeliveryResult == Result.Successful);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public async Task ListDeliveryAttempts_PackageWithoutAttempts_ReturnsNotFound(int packageId)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);

        // Act
        var response = await client.GetAsync($"api/v1.0/admin/packages/{packageId}/delivery-attempts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var message = (await response.Content.ReadFromJsonAsync<MessageResponse>())?.Message;
        message.Should().Be($"No delivery attempts found for Package with Id {packageId}");
    }

    #endregion
}
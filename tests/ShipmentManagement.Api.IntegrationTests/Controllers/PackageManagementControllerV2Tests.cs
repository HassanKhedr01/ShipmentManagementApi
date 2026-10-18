using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using FluentValidation;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using ShipmentManagement.Api.IntegrationTests.Helper;
using ShipmentManagement.Api.IntegrationTests.TestHost;
using ShipmentManagement.Application.DTOs.Common;
using ShipmentManagement.Application.DTOs.DeliveryAttempts;
using ShipmentManagement.Application.DTOs.Packages;
using ShipmentManagement.Application.DTOs.TrackingEvents;
using ShipmentManagement.Infrastructure.Persistence;

namespace ShipmentManagement.Api.IntegrationTests.Controllers;

public class PackageManagementControllerV2Tests
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    #region PackageManagementRequests

    [Theory]
    [InlineData("GET", "api/v2.0/admin/packages")]
    [InlineData("GET", "api/v2.0/admin/packages/1")]
    [InlineData("GET", "api/v2.0/admin/packages/1/events")]
    [InlineData("POST", "api/v2.0/admin/packages/1/events")]
    [InlineData("GET", "api/v2.0/admin/packages/1/delivery-attempts")]
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
    [InlineData("GET", "api/v2.0/admin/packages")]
    [InlineData("GET", "api/v2.0/admin/packages/1")]
    [InlineData("GET", "api/v2.0/admin/packages/1/events")]
    [InlineData("POST", "api/v2.0/admin/packages/1/events")]
    [InlineData("GET", "api/v2.0/admin/packages/1/delivery-attempts")]
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
    public async Task ListPackages_AsAdmin_ReturnsAllPackagesWithDeliveryType()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);

        // Act
        var response = await client.GetAsync("api/v2.0/admin/packages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var packages = await response.Content.ReadFromJsonAsync<List<PackageV2Dto>>(_jsonOptions);

        packages.Should().NotBeNull();
        packages.Should().HaveCount(7);
        packages.Should().OnlyContain(p =>
            p.DeliveryType == DeliveryType.Standard ||
            p.DeliveryType == DeliveryType.Express);

        packages.Should().Contain(p => p.DeliveryType == DeliveryType.Standard);
        packages.Should().Contain(p => p.DeliveryType == DeliveryType.Express);
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
        var response = await client.GetAsync("api/v2.0/admin/packages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(1, DeliveryType.Standard)]
    [InlineData(2, DeliveryType.Express)]
    [InlineData(3, DeliveryType.Standard)]
    [InlineData(4, DeliveryType.Express)]
    [InlineData(5, DeliveryType.Standard)]
    [InlineData(6, DeliveryType.Standard)]
    [InlineData(7, DeliveryType.Express)]
    public async Task GetPackage_ExistingPackage_ReturnsPackageWithDeliveryType(
        int packageId, DeliveryType expectedDeliveryType)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);

        // Act
        var response = await client.GetAsync($"api/v2.0/admin/packages/{packageId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var package = await response.Content.ReadFromJsonAsync<PackageV2Dto>(_jsonOptions);
        package.Should().NotBeNull();
        package.Id.Should().Be(packageId);
        package.DeliveryType.Should().Be(expectedDeliveryType);
    }

    [Fact]
    public async Task GetPackage_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        const int packageId = int.MaxValue;

        // Act
        var response = await client.GetAsync($"api/v2.0/admin/packages/{packageId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region TrackingEventRetrieval

    [Fact]
    public async Task TrackPackage_ExistingPackageWithEvents_ReturnsEvents()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        const int packageId = 1;

        // Act
        var response = await client.GetAsync($"api/v2.0/admin/packages/{packageId}/events");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var events = await response.Content
            .ReadFromJsonAsync<List<TrackingEventDto>>(_jsonOptions);

        events.Should().NotBeNull();
        events.Should().HaveCount(2);
    }

    [Fact]
    public async Task TrackPackage_PackageWithoutEvents_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        const int packageId = int.MaxValue;

        // Act
        var response = await client.GetAsync($"api/v2.0/admin/packages/{packageId}/events");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region RecordTrackingEvent

    [Fact]
    public async Task RecordEvent_ValidTransition_ReturnsEventAndUpdatesPackage()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        const int packageId = 1;

        var request = new CreateTrackingEventDto
        {
            NewStatus = Status.InTransit, Description = "Package is in transit", FacilityId = 1
        };

        // Act
        var response = await client.PostAsJsonAsync($"api/v2.0/admin/packages/{packageId}/events", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var trackingEvent = await response.Content.ReadFromJsonAsync<TrackingEventDto>(_jsonOptions);
        trackingEvent.Should().NotBeNull();
        trackingEvent.Status.Should().Be(Status.InTransit);
        trackingEvent.Description.Should().Be(request.Description);
        var updatedPackage = await context.Packages.FirstOrDefaultAsync(p => p.Id == packageId);
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
        const int packageId = 6;

        var request = new CreateTrackingEventDto
        {
            NewStatus = Status.Delivered, Description = "Package delivered successfully"
        };

        // Act
        var response = await client.PostAsJsonAsync($"api/v2.0/admin/packages/{packageId}/events", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedPackage = await context.Packages.FirstOrDefaultAsync(p => p.Id == packageId);
        updatedPackage.Should().NotBeNull();
        updatedPackage.CurrentStatus.Should().Be(Domain.Enums.Status.Delivered);
        var attempt = await context.DeliveryAttempts
            .FirstOrDefaultAsync(a => a.PackageId == packageId && a.DeliveryResult == Domain.Enums.Result.Successful);
        attempt.Should().NotBeNull();
    }

    [Fact]
    public async Task RecordEvent_InvalidCreateRequest_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);

        var request = new CreateTrackingEventDto { NewStatus = (Status)999, Description = new string('D', 257) };
        var validator = factory.Services.GetRequiredService<IValidator<CreateTrackingEventDto>>();
        var validationResults = await validator.ValidateAsync(request);

        // Act
        var response = await client.PostAsJsonAsync("api/v2.0/admin/packages/1/events", request);

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
        const int packageId = int.MaxValue;

        var request = new CreateTrackingEventDto { NewStatus = Status.PickedUp, Description = "Package picked up" };

        // Act
        var response = await client.PostAsJsonAsync($"api/v2.0/admin/packages/{packageId}/events", request);

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
        var response = await client.GetAsync(
            $"api/v2.0/admin/packages/{packageId}/delivery-attempts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var attempts = await response.Content.ReadFromJsonAsync<List<DeliveryAttemptDto>>(_jsonOptions);
        attempts.Should().NotBeNull();
        attempts.Should().HaveCount(2);
        attempts.Should().Contain(a => a.DeliveryResult == Result.Failed);
        attempts.Should().Contain(a => a.DeliveryResult == Result.Successful);
    }

    [Fact]
    public async Task ListDeliveryAttempts_PackageWithoutAttempts_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        const int packageId = 1;

        // Act
        var response = await client.GetAsync($"api/v2.0/admin/packages/{packageId}/delivery-attempts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
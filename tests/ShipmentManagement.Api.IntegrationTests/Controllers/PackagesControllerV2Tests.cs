using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

using ShipmentManagement.Api.IntegrationTests.Common;
using ShipmentManagement.Api.IntegrationTests.Helper;
using ShipmentManagement.Api.IntegrationTests.TestHost;
using ShipmentManagement.Api.Models;
using ShipmentManagement.Application.DTOs.Common;
using ShipmentManagement.Application.DTOs.Packages;

namespace ShipmentManagement.Api.IntegrationTests.Controllers;

public class PackagesControllerV2Tests
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    #region PackageRequests

    [Theory]
    [InlineData("GET", "api/v2.0/packages/1")]
    [InlineData("GET", "api/v2.0/packages/")]
    [InlineData("POST", "api/v2.0/packages/create")]
    [InlineData("GET", "api/v2.0/packages/1/tracking")]
    [InlineData("PATCH", "api/v2.0/packages/1/cancel")]
    public async Task PackageRequest_NotAuthenticated_ReturnsUnauthorized(string method, string url)
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

    #endregion

    #region GetPackage

    [Theory]
    [InlineData(1, DeliveryType.Standard)]
    [InlineData(2, DeliveryType.Express)]
    public async Task GetPackage_PackageExists_ReturnsPackageWithDeliveryType(
        int packageId, DeliveryType expectedDeliveryType)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);

        // Act
        var response = await client.GetAsync($"api/v2.0/packages/{packageId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var package = await response.Content.ReadFromJsonAsync<PackageV2Dto>(_jsonOptions);
        package.Should().NotBeNull();
        package.Id.Should().Be(packageId);
        package.DeliveryType.Should().Be(expectedDeliveryType);
    }

    [Fact]
    public async Task GetPackage_PackageDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);
        const int packageId = int.MaxValue;

        // Act
        var response = await client.GetAsync($"api/v2.0/packages/{packageId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        messageResponse?.Message.Should().Be("Package not found");
    }

    #endregion

    #region GetAllPackages

    [Fact]
    public async Task GetAllPackages_PackagesExist_ReturnsPackagesWithDeliveryTypes()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);

        // Act
        var response = await client.GetAsync("api/v2.0/packages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var packages = await response.Content.ReadFromJsonAsync<List<PackageV2Dto>>(_jsonOptions);
        packages.Should().NotBeNull();
        packages.Should().NotBeEmpty();
        packages.Should().OnlyContain(p => Enum.IsDefined(p.DeliveryType));
        packages.Should().Contain(p => p.DeliveryType == DeliveryType.Standard);
        packages.Should().Contain(p => p.DeliveryType == DeliveryType.Express);
    }

    [Fact]
    public async Task GetAllPackages_PackagesDoNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.RegisterAndCreateAsync(factory);

        // Act
        var response = await client.GetAsync("api/v2.0/packages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        messageResponse?.Message.Should().Be("No packages found");
    }

    #endregion

    #region CreatePackage

    [Theory]
    [InlineData(DeliveryType.Standard, 3)]
    [InlineData(DeliveryType.Express, 1)]
    public async Task CreatePackage_ValidDeliveryType_ReturnsCreatedPackage(
        DeliveryType deliveryType, int expectedDeliveryDays)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);

        var request = new CreatePackageV2Dto
        {
            Name = "Test Package",
            SenderName = "Egy Labs",
            RecipientName = "Omar Ahmed",
            OriginAddress = new AddressDto { Street = "Origin Street", City = "Cairo", PostalCode = "11111" },
            DestinationAddress = new AddressDto
            {
                Street = "Destination Street", City = "Alexandria", PostalCode = "22222"
            },
            DeliveryType = deliveryType
        };

        // Act
        var response = await client.PostAsJsonAsync("api/v2.0/packages/create", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var package = await response.Content.ReadFromJsonAsync<PackageV2Dto>(_jsonOptions);
        package.Should().NotBeNull();
        package.Should().BeEquivalentTo(request, options => options.ExcludingMissingMembers());
        package.DeliveryType.Should().Be(deliveryType);

        package.EstimatedDeliveryDate.Date.Should().Be(DateTime.UtcNow.AddDays(expectedDeliveryDays).Date);
    }

    [Fact]
    public async Task CreatePackage_InvalidDeliveryType_ReturnsBadRequestWithErrors()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);

        var request = new CreatePackageV2Dto
        {
            Name = "Test Package",
            SenderName = "Egy Labs",
            RecipientName = "Omar Ahmed",
            OriginAddress = new AddressDto { Street = "Origin Street", City = "Cairo", PostalCode = "11111" },
            DestinationAddress = new AddressDto
            {
                Street = "Destination Street", City = "Alexandria", PostalCode = "22222"
            },
            DeliveryType = (DeliveryType)999
        };

        var validator = factory.Services.GetRequiredService<IValidator<CreatePackageV2Dto>>();
        var validationResult = await validator.ValidateAsync(request);

        // Act
        var response = await client.PostAsJsonAsync("api/v2.0/packages/create", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errors = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().BeEquivalentTo(validationResult.ToDictionary());
    }

    #endregion

    #region TrackPackage

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task TrackPackage_PackageHasTrackingEvents_ReturnsTrackingEvents(int packageId)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);

        // Act
        var response = await client.GetAsync($"api/v2.0/packages/{packageId}/tracking");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var trackingEvents = await response.Content.ReadFromJsonAsync<List<UserTrackingEvent>>(_jsonOptions);
        trackingEvents.Should().NotBeNull();
        trackingEvents.Should().NotBeEmpty();
        trackingEvents.Should().OnlyContain(e => e.PackageId == packageId);
    }

    [Fact]
    public async Task TrackPackage_PackageHasNoTrackingEvents_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.RegisterAndCreateAsync(factory);
        const int packageId = 1;

        // Act
        var response = await client.GetAsync($"api/v2.0/packages/{packageId}/tracking");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        messageResponse?.Message.Should().Be("No tracking events found for the specified package.");
    }

    #endregion

    #region CancelPackage

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public async Task CancelPackage_PackageCanBeCanceled_ReturnsNoContent(int packageId)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);

        // Act
        var response = await client.PatchAsync($"api/v2.0/packages/{packageId}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(int.MaxValue)]
    public async Task CancelPackage_PackageCannotBeCanceled_ReturnsUnprocessableEntity(int packageId)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);

        // Act
        var response = await client.PatchAsync($"api/v2.0/packages/{packageId}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        messageResponse?.Message.Should().Be("Could not cancel the package.");
    }

    #endregion
}
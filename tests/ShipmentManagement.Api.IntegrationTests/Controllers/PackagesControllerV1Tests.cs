using System.Diagnostics.CodeAnalysis;
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

public class PackagesControllerV1Tests
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerOptions.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Theory]
    [InlineData("GET", "api/v1.0/packages/1")]
    [InlineData("GET", "api/v1.0/packages/")]
    [InlineData("POST", "api/v1.0/packages/create")]
    [InlineData("GET", "api/v1.0/packages/1/tracking")]
    [InlineData("PATCH", "api/v1.0/packages/1/cancel")]
    public async Task PackageRequest_NotAuthenticated_ReturnsUnauthorized(string method, string url)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = factory.CreateClient();
        var httpRequest = new HttpRequestMessage(new HttpMethod(method), url);

        // Act
        var response = await client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #region GetPackage

    [Theory] // user's packages
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task GetPackage_PackageExists_ReturnsOkWithThePackage(int packageId)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);

        // Act
        var response = await client.GetAsync($"api/v1.0/packages/{packageId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var package = await response.Content.ReadFromJsonAsync<PackageV1Dto>(_jsonOptions);
        package.Should().NotBeNull();
        package.Id.Should().Be(packageId);
    }

    [Fact]
    public async Task GetPackage_PackageDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);
        const int packageId = int.MaxValue;

        // Act
        var response = await client.GetAsync($"api/v1.0/Packages/{packageId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        var message = messageResponse?.Message;
        message.Should().NotBeNull();
        message.Should().Be("Package not found");
    }

    #endregion

    #region GetAllPackages

    [Fact]
    public async Task GetAllPackages_PackagesExists_ReturnsOkWithThePackages()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);

        // Act
        var response = await client.GetAsync("api/v1.0/packages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var packages = await response.Content.ReadFromJsonAsync<List<PackageV1Dto>>(_jsonOptions);
        packages.Should().NotBeNull();
        packages.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAllPackages_PackagesDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client =
            await AuthenticatedClientFactory.RegisterAndCreateAsync(factory); // register a new user with no packages

        // Act
        var response = await client.GetAsync("api/v1.0/Packages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        var message = messageResponse?.Message;
        message.Should().NotBeNull();
        message.Should().Be("No packages found");
    }

    #endregion

    #region CreatePackage

    [Fact]
    public async Task CreatePackage_ValidCreateRequest_ReturnsOkWithThePackage()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);
        var request = new CreatePackageV1Dto
        {
            Name = "Test Package",
            SenderName = "Egy Labs",
            RecipientName = "Omar Ahmed",
            OriginAddress = new AddressDto { Street = "Street test", City = "City test", PostalCode = "11111" },
            DestinationAddress = new AddressDto { Street = "Street test", City = "City test", PostalCode = "11111" }
        };

        // Act
        var response = await client.PostAsJsonAsync("api/v1.0/packages/create", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var package = await response.Content.ReadFromJsonAsync<PackageV1Dto>(_jsonOptions);
        package.Should().NotBeNull();
        package.Should().BeEquivalentTo(request, options => options.ExcludingMissingMembers());
    }

    [Theory]
    [MemberData(nameof(InvalidPackageData))]
    public async Task CreatePackage_RequestWithValidationErrors_ReturnsBadRequestWithErrors(CreatePackageV1Dto request)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);
        var validator = factory.Services.GetRequiredService<IValidator<CreatePackageV1Dto>>();
        var validationResult = await validator.ValidateAsync(request);

        // Act
        var response = await client.PostAsJsonAsync("api/v1.0/packages/create", request);

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
    public async Task TrackPackage_PackageHasTrackingEvents_ReturnsOkWithTheTrackingEvents(int packageId)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory);

        // Act
        var response = await client.GetAsync($"api/v1.0/packages/{packageId}/tracking");

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
        var response = await client.GetAsync($"api/v1.0/packages/{packageId}/tracking");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        var message = messageResponse?.Message;
        message.Should().NotBeNull();
        message.Should().Be("No tracking events found for the package.");
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
        var response = await client.PatchAsync($"api/v1.0/packages/{packageId}/cancel", null);

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
        var response = await client.PatchAsync($"api/v1.0/packages/{packageId}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        var message = messageResponse?.Message;
        message.Should().NotBeNull();
        message.Should().Be("Could not cancel the package.");
    }

    #endregion

    [SuppressMessage("ReSharper", "UseCollectionExpression")]
    public static IEnumerable<object[]> InvalidPackageData()
    {
        yield return new object[]
        {
            new CreatePackageV1Dto
            {
                Name = string.Empty,
                SenderName = string.Empty,
                RecipientName = string.Empty,
                OriginAddress =
                    new AddressDto { Street = string.Empty, City = string.Empty, PostalCode = string.Empty },
                DestinationAddress = new AddressDto
                {
                    Street = new string('S', 101), City = string.Empty, PostalCode = new string('1', 21)
                }
            }
        };
    }
}
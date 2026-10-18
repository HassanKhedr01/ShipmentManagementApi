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
using ShipmentManagement.Application.DTOs.Facilities;
using ShipmentManagement.Infrastructure.Persistence;

namespace ShipmentManagement.Api.IntegrationTests.Controllers;

public class FacilitiesControllerV1Tests
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    #region FacilityRequests

    [Theory]
    [InlineData("GET", "api/v1.0/admin/facilities/1")]
    [InlineData("GET", "api/v1.0/admin/facilities/")]
    [InlineData("POST", "api/v1.0/admin/facilities/add")]
    [InlineData("PATCH", "api/v1.0/admin/facilities/1")]
    [InlineData("PATCH", "api/v1.0/admin/facilities/1/activate")]
    [InlineData("PATCH", "api/v1.0/admin/facilities/1/deactivate")]
    public async Task FacilityRequest_NotAuthenticated_ReturnsUnauthorized(string method, string url)
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

    [Theory]
    [InlineData("GET", "api/v1.0/admin/facilities/1")]
    [InlineData("GET", "api/v1.0/admin/facilities/")]
    [InlineData("POST", "api/v1.0/admin/facilities/add")]
    [InlineData("PATCH", "api/v1.0/admin/facilities/1")]
    [InlineData("PATCH", "api/v1.0/admin/facilities/1/activate")]
    [InlineData("PATCH", "api/v1.0/admin/facilities/1/deactivate")]
    public async Task FacilityRequest_NotAdmin_ReturnsForbidden(string method, string url)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory); // not admin
        var httpRequest = new HttpRequestMessage(new HttpMethod(method), url);

        // Act
        var response = await client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region GetFacility

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task GetFacility_FacilityExists_ReturnsOkWithTheFacility(int facilityId)
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);

        // Act
        var response = await client.GetAsync($"api/v1.0/admin/facilities/{facilityId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var facility = await response.Content.ReadFromJsonAsync<FacilityDto>(_jsonOptions);
        facility.Should().NotBeNull();
        facility.Id.Should().Be(facilityId);
    }

    [Fact]
    public async Task GetFacility_FacilityDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        const int facilityId = int.MaxValue;

        // Act
        var response = await client.GetAsync($"api/v1.0/admin/facilities/{facilityId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        var message = messageResponse?.Message;
        message.Should().NotBeNull();
        message.Should().Be($"Facility with Id {facilityId} is not found");
    }

    #endregion

    #region GetAllFacilities

    [Fact]
    public async Task GetAllFacilities_FacilitiesExist_ReturnsOkWithTheFacilities()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);

        // Act
        var response = await client.GetAsync("api/v1.0/admin/facilities");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var facilities = await response.Content.ReadFromJsonAsync<List<FacilityDto>>(_jsonOptions);
        facilities.Should().NotBeNull();
        facilities.Should().HaveCount(5);
    }

    #endregion

    #region AddFacility

    [Fact]
    public async Task AddFacility_ValidCreateRequest_ReturnsCreatedWithTheFacility()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        var request = new CreateFacilityDto { Name = "Test Facility", Address = "25 Test Street", City = "Cairo" };

        // Act
        var response = await client.PostAsJsonAsync("api/v1.0/admin/facilities/add", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var facility = await response.Content.ReadFromJsonAsync<FacilityDto>(_jsonOptions);
        facility.Should().NotBeNull();
        facility.Id.Should().BeGreaterThan(0);
        facility.Should().BeEquivalentTo(request, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task AddFacility_RequestWithValidationErrors_ReturnsBadRequestWithErrors()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        var request = new CreateFacilityDto { Name = string.Empty, Address = string.Empty, City = string.Empty };
        var validator = factory.Services.GetRequiredService<IValidator<CreateFacilityDto>>();
        var validationResult = await validator.ValidateAsync(request);

        // Act
        var response = await client.PostAsJsonAsync("api/v1.0/admin/facilities/add", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errors = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().BeEquivalentTo(validationResult.ToDictionary());
    }

    #endregion

    #region UpdateFacility

    [Fact]
    public async Task UpdateFacility_ValidUpdateRequest_ReturnsNoContent()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        const int facilityId = 1;
        var request = new UpdateFacilityDto
        {
            Name = "Updated Cairo Hub", Address = "30 Updated Street", City = "Cairo"
        };

        // Act
        var response = await client.PatchAsJsonAsync($"api/v1.0/admin/facilities/{facilityId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updatedFacility = await context.Facilities.FindAsync(facilityId);
        updatedFacility.Should().NotBeNull();
        updatedFacility.Should().NotBeEquivalentTo(request, options => options.ExcludingMissingMembers());
    }

    [Fact]
    public async Task UpdateFacility_FacilityCannotBeUpdated_ReturnsUnprocessableEntity()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        const int facilityId = int.MaxValue;
        var request = new UpdateFacilityDto
        {
            Name = "Updated Facility", Address = "30 Updated Street", City = "Cairo"
        };

        // Act
        var response = await client.PatchAsJsonAsync($"api/v1.0/admin/facilities/{facilityId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        var message = messageResponse?.Message;
        message.Should().NotBeNull();
        message.Should().Be("Could not update the facility");
    }

    [Fact]
    public async Task UpdateFacility_RequestWithValidationErrors_ReturnsBadRequestWithErrors()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        var request = new UpdateFacilityDto { Name = string.Empty, Address = string.Empty, City = string.Empty };
        var validator = factory.Services.GetRequiredService<IValidator<UpdateFacilityDto>>();
        var validationResult = await validator.ValidateAsync(request);

        // Act
        var response = await client.PatchAsJsonAsync("api/v1.0/admin/facilities/1", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errors = await response.Content.ReadFromJsonAsync<Dictionary<string, string[]>>(_jsonOptions);
        errors.Should().BeEquivalentTo(validationResult.ToDictionary());
    }

    #endregion

    #region ActivateFacility

    [Fact]
    public async Task ActivateFacility_InactiveFacility_ReturnsNoContentWithFacilityActivated()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        const int facilityId = 5;

        // Act
        var response = await client.PatchAsync($"api/v1.0/admin/facilities/{facilityId}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updatedFacility = await context.Facilities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == facilityId);
        updatedFacility.Should().NotBeNull();
        updatedFacility.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateFacility_FacilityDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        const int facilityId = int.MaxValue;

        // Act
        var response = await client.PatchAsync($"api/v1.0/admin/facilities/{facilityId}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        var message = messageResponse?.Message;
        message.Should().NotBeNull();
        message.Should().Be($"Facility with Id {facilityId} is not found");
    }

    #endregion

    #region DeactivateFacility

    [Fact]
    public async Task DeactivateFacility_ActiveFacility_ReturnsNoContentWithFacilityDeactivated()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        const int facilityId = 1;

        // Act
        var response = await client.PatchAsync($"api/v1.0/admin/facilities/{facilityId}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updatedFacility = await context.Facilities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == facilityId);
        updatedFacility.Should().NotBeNull();
        updatedFacility.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateFacility_FacilityDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var factory = new CustomWebApplicationFactory();
        using var client = await AuthenticatedClientFactory.CreateAsync(factory, true);
        const int facilityId = int.MaxValue;

        // Act
        var response = await client.PatchAsync($"api/v1.0/admin/facilities/{facilityId}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var messageResponse = await response.Content.ReadFromJsonAsync<MessageResponse>();
        var message = messageResponse?.Message;
        message.Should().NotBeNull();
        message.Should().Be($"Facility with Id {facilityId} is not found");
    }

    #endregion
}
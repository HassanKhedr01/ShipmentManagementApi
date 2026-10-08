using FluentAssertions;

using ShipmentManagement.Application.DTOs.Facilities;
using ShipmentManagement.Application.Mappings;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Testing.Common;

namespace ShipmentManagement.Application.Tests.Mappings;

public class FacilityMappingExtensionsTest
{
    [Theory, AutoMoqData]
    public void ToResult_WhenCalled_ShouldMapFacilityToFacilityResult(Facility facility)
    {
        // Act
        var result = facility.ToResult();

        // Assert
        result.Should().BeEquivalentTo(facility, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public void ToDto_WhenCalled_ShouldMapFacilityResultToFacilityDto(FacilityResult facilityResult)
    {
        // Act
        var result = facilityResult.ToDto();

        // Assert
        result.Should().BeEquivalentTo(facilityResult, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public void ToEntity_WhenCalled_ShouldMapCreateFacilityRequestToFacility(
        CreateFacilityRequest request)
    {
        // Act
        var result = request.ToEntity();

        // Assert
        result.Should().BeEquivalentTo(request, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public void ToCreateRequest_WhenCalled_ShouldMapCreateFacilityDtoToCreateFacilityRequest(
        CreateFacilityDto dto)
    {
        // Act
        var result = dto.ToCreateRequest();

        // Assert
        result.Should().BeEquivalentTo(dto, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public void ToEntity_WhenCalled_ShouldMapUpdateFacilityRequestToFacility(
        UpdateFacilityRequest request)
    {
        // Act
        var result = request.ToEntity();

        // Assert
        result.Should().BeEquivalentTo(request, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public void ToUpdateRequest_WhenCalled_ShouldMapUpdateFacilityDtoToUpdateFacilityRequest(
        UpdateFacilityDto dto)
    {
        // Act
        var result = dto.ToUpdateRequest();

        // Assert
        result.Should().BeEquivalentTo(dto, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public void ToEntity_WhenCalled_ShouldMapFacilityResultToFacility(FacilityResult facilityResult)
    {
        // Act
        var result = facilityResult.ToEntity();

        // Assert
        result.Should().BeEquivalentTo(facilityResult, options => options.ExcludingMissingMembers());
    }
}
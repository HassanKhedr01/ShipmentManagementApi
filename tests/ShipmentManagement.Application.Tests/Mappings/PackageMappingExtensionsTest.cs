using FluentAssertions;

using ShipmentManagement.Application.DTOs.Packages;
using ShipmentManagement.Application.Mappings;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Testing.Common;

namespace ShipmentManagement.Application.Tests.Mappings;

public class PackageMappingExtensionsTest
{
    [Theory, AutoMoqData]
    public void ToResult_WhenCalled_ReturnsCorrectMappingFromEntityToResult(Package package)
    {
        // Act
        var result = package.ToResult();

        // Assert
        result.Should().BeEquivalentTo(package, options => options.ExcludingMissingMembers());
    }
    
    [Theory, AutoMoqData]
    public void ToV1Dto_WhenCalled_ReturnsCorrectMappingFromResultToV1Dto(PackageResult packageResult)
    {
        // Act
        var result = packageResult.ToV1Dto();

        // Assert
        result.Should().BeEquivalentTo(packageResult, options => options.ExcludingMissingMembers());
    }
    
    [Theory, AutoMoqData]
    public void ToV2Dto_WhenCalled_ReturnsCorrectMappingFromResultToV2Dto(PackageResult packageResult)
    {
        // Act
        var result = packageResult.ToV2Dto();

        // Assert
        result.Should().BeEquivalentTo(packageResult, options => options.ExcludingMissingMembers());
    }
    
    [Theory, AutoMoqData]
    public void ToEntity_WhenCalled_ReturnsCorrectMappingFromCreateRequestToEntity(CreatePackageRequest createRequest)
    {
        // Act
        var result = createRequest.ToEntity();

        // Assert
        result.Should().BeEquivalentTo(createRequest, options => options.ExcludingMissingMembers());
    }
    
    [Theory, AutoMoqData]
    public void ToCreateRequest_WhenCalled_ReturnsCorrectMappingFromCreateV1DtoToCreateRequest(CreatePackageV1Dto createV1Dto)
    {
        // Act
        var result = createV1Dto.ToCreateRequest();

        // Assert
        result.Should().BeEquivalentTo(createV1Dto, options => options.ExcludingMissingMembers());
    }
    
    [Theory, AutoMoqData]
    public void ToCreateRequest_WhenCalled_ReturnsCorrectMappingFromCreateV2DtoToCreateRequest(CreatePackageV2Dto createV2Dto)
    {
        // Act
        var result = createV2Dto.ToCreateRequest();

        // Assert
        result.Should().BeEquivalentTo(createV2Dto, options => options.ExcludingMissingMembers());
    }
}
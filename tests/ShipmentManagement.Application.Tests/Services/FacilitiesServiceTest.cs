using AutoFixture.Xunit2;

using FluentAssertions;

using Moq;

using ShipmentManagement.Application.DTOs.Facilities;
using ShipmentManagement.Application.Services;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Domain.Repositories;
using ShipmentManagement.Testing.Common;

namespace ShipmentManagement.Application.Tests.Services;

public class FacilitiesServiceTest
{
    #region GetFacilityByIdAsync

    [Theory, AutoMoqData]
    public async Task GetFacilityByIdAsync_FacilityExists_ReturnsFacility(
        int facilityId,
        Facility facility,
        [Frozen] Mock<IFacilitiesRepository> mockRepo,
        FacilitiesService service)
    {
        // Arrange
        facility.Id = facilityId;
        mockRepo.Setup(repo => repo.GetByIdAsync(facilityId)).ReturnsAsync(facility);

        // Act
        var result = await service.GetFacilityByIdAsync(facilityId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(facility, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public async Task GetFacilityByIdAsync_IdIsNull_ReturnsNull(
        FacilitiesService service)
    {
        // Act
        var result = await service.GetFacilityByIdAsync(null);

        // Assert
        result.Should().BeNull();
    }

    [Theory, AutoMoqData]
    public async Task GetFacilityByIdAsync_FacilityDoesNotExist_ReturnsNull(
        [Frozen] Mock<IFacilitiesRepository> mockRepo,
        FacilitiesService service)
    {
        // Arrange
        mockRepo.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Facility?)null);

        // Act
        var result = await service.GetFacilityByIdAsync(null);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllFacilitiesAsync

    [Theory, AutoMoqData]
    public async Task GetAllFacilitiesAsync_FacilitiesExist_ReturnsFacilities(
        List<Facility> facilities,
        [Frozen] Mock<IFacilitiesRepository> mockRepo,
        FacilitiesService service)
    {
        // Arrange
        mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(facilities);

        // Act
        var result = await service.GetAllFacilitiesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(facilities.Count);
        result.Should().BeEquivalentTo(facilities, options => options.ExcludingMissingMembers());
    }

    [Theory, AutoMoqData]
    public async Task GetAllFacilitiesAsync_NoFacilitiesExist_ReturnsEmptyList(
        [Frozen] Mock<IFacilitiesRepository> mockRepo,
        FacilitiesService service)
    {
        // Arrange
        mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Facility>());

        // Act
        var result = await service.GetAllFacilitiesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    #endregion

    #region AddFacilityAsync

    [Theory, AutoMoqData]
    public async Task AddFacilityAsync_ValidRequest_AddsFacility(
        CreateFacilityRequest createRequest,
        Facility facility,
        [Frozen] Mock<IFacilitiesRepository> mockRepo,
        FacilitiesService service)
    {
        // Arrange
        mockRepo.Setup(repo => repo.AddAsync(It.IsAny<Facility>())).ReturnsAsync(facility);

        // Act
        var result = await service.AddFacilityAsync(createRequest);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(facility, options => options.ExcludingMissingMembers());
    }

    #endregion

    #region UpdateFacilityAsync

    [Theory, AutoMoqData]
    public async Task UpdateFacilityAsync_ValidUpdateRequest_UpdatesFacility(
        int facilityId,
        UpdateFacilityRequest updateRequest,
        Facility existingFacility,
        [Frozen] Mock<IFacilitiesRepository> mockRepo,
        FacilitiesService service)
    {
        // Arrange
        mockRepo.Setup(repo => repo.GetByIdAsync(facilityId)).ReturnsAsync(existingFacility);
        mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Facility>())).ReturnsAsync(true);

        // Act
        var result = await service.UpdateFacilityAsync(facilityId, updateRequest);

        // Assert
        result.Should().BeTrue();
    }

    [Theory, AutoMoqData]
    public async Task UpdateFacilityAsync_InvalidUpdateRequest_DoesNotUpdateFacility(
        int facilityId,
        UpdateFacilityRequest updateRequest,
        Facility existingFacility,
        [Frozen] Mock<IFacilitiesRepository> mockRepo,
        FacilitiesService service)
    {
        // Arrange
        mockRepo.Setup(repo => repo.GetByIdAsync(facilityId)).ReturnsAsync(existingFacility);
        mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Facility>())).ReturnsAsync(false);

        // Act
        var result = await service.UpdateFacilityAsync(facilityId, updateRequest);

        // Assert
        result.Should().BeFalse();
    }

    [Theory, AutoMoqData]
    public async Task UpdateFacilityAsync_FacilityDoesNotExist_ReturnsFalse(
        int facilityId,
        UpdateFacilityRequest updateRequest,
        [Frozen] Mock<IFacilitiesRepository> mockRepo,
        FacilitiesService service)
    {
        // Arrange
        mockRepo.Setup(repo => repo.GetByIdAsync(facilityId)).ReturnsAsync((Facility?)null);

        // Act
        var result = await service.UpdateFacilityAsync(facilityId, updateRequest);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsFacilityAvailableForOperations

    [Theory, AutoMoqData]
    public void IsFacilityAvailableForOperations_FacilityIsActive_ReturnsTrue(
        FacilityResult facilityDto,
        FacilitiesService service)
    {
        // Arrange
        facilityDto.IsActive = true;

        // Act
        var result = service.IsFacilityAvailableForOperations(facilityDto);

        // Assert
        result.Should().BeTrue();
    }

    [Theory, AutoMoqData]
    public void IsFacilityAvailableForOperations_FacilityIsNotActive_ReturnsFalse(
        FacilityResult facilityDto,
        FacilitiesService service)
    {
        // Arrange
        facilityDto.IsActive = false;

        // Act
        var result = service.IsFacilityAvailableForOperations(facilityDto);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ActivateFacilityAsync

    [Theory, AutoMoqData]
    public async Task ActivateFacilityAsync_FacilityIsInactive_ActivatesFacility(
        FacilityResult facilityDto,
        [Frozen] Mock<IFacilitiesRepository> mockRepo,
        FacilitiesService service)
    {
        // Arrange
        facilityDto.IsActive = false;
        mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Facility>())).ReturnsAsync(true);

        // Act
        var result = await service.ActivateFacilityAsync(facilityDto);

        // Assert
        result.Should().BeTrue();
    }

    [Theory, AutoMoqData]
    public async Task ActivateFacilityAsync_FacilityIsAlreadyActive_ReturnsTrue(
        FacilityResult facilityDto,
        FacilitiesService service)
    {
        // Arrange
        facilityDto.IsActive = true;

        // Act
        var result = await service.ActivateFacilityAsync(facilityDto);

        // Assert
        result.Should().BeTrue();
    }
    
    [Theory, AutoMoqData]
    public async Task ActivateFacilityAsync_FailureToUpdateFacilityActivation_ReturnsFalse(
        FacilityResult facilityDto,
        [Frozen] Mock<IFacilitiesRepository> mockRepo,
        FacilitiesService service)
    {
        // Arrange
        facilityDto.IsActive = false;
        mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Facility>())).ReturnsAsync(false);

        // Act
        var result = await service.ActivateFacilityAsync(facilityDto);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region DeactivateFacilityAsync

    [Theory, AutoMoqData]
    public async Task DeactivateFacilityAsync_FacilityIsActive_DeactivatesFacility(
        FacilityResult facilityDto,
        [Frozen] Mock<IFacilitiesRepository> mockRepo,
        FacilitiesService service)
    {
        // Arrange
        facilityDto.IsActive = true;
        mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Facility>())).ReturnsAsync(true);

        // Act
        var result = await service.DeactivateFacilityAsync(facilityDto);

        // Assert
        result.Should().BeTrue();
    }
    
    [Theory, AutoMoqData]
    public async Task DeactivateFacilityAsync_FacilityIsAlreadyInactive_ReturnsTrue(
        FacilityResult facilityDto,
        FacilitiesService service)
    {
        // Arrange
        facilityDto.IsActive = false;

        // Act
        var result = await service.DeactivateFacilityAsync(facilityDto);

        // Assert
        result.Should().BeTrue();
    }
    
    [Theory, AutoMoqData]
    public async Task DeactivateFacilityAsync_FailureToUpdateFacilityActivation_ReturnsFalse(
        FacilityResult facilityDto,
        [Frozen] Mock<IFacilitiesRepository> mockRepo,
        FacilitiesService service)
    {
        // Arrange
        facilityDto.IsActive = true;
        mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<Facility>())).ReturnsAsync(false);

        // Act
        var result = await service.DeactivateFacilityAsync(facilityDto);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
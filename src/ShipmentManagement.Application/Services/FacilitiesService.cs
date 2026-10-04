using Microsoft.Extensions.Logging;

using ShipmentManagement.Application.DTOs.Facilities;
using ShipmentManagement.Application.Interfaces;
using ShipmentManagement.Application.Mappings;
using ShipmentManagement.Domain.Repositories;

namespace ShipmentManagement.Application.Services;

public class FacilitiesService : IFacilitiesService
{
    private readonly ILogger<FacilitiesService> _logger;
    private readonly IFacilitiesRepository _facilitiesRepository;

    public FacilitiesService(ILogger<FacilitiesService> logger, IFacilitiesRepository facilitiesRepository)
    {
        _logger = logger;
        _facilitiesRepository = facilitiesRepository;
    }

    public async Task<FacilityResult?> GetFacilityByIdAsync(int? id)
    {
        _logger.LogDebug("Getting facility with ID {FacilityId}", id);
        if (id == null)
        {
            _logger.LogWarning("Facility ID is null.");
            return null;
        }

        var facility = await _facilitiesRepository.GetByIdAsync(id);
        if (facility == null)
        {
            _logger.LogWarning("Facility with ID {FacilityId} not found.", id);
            return null;
        }

        _logger.LogInformation("Retrieved facility {FacilityName} with ID {FacilityId}", facility.Name, facility.Id);
        return facility.ToResult();
    }

    public async Task<List<FacilityResult>> GetAllFacilitiesAsync()
    {
        _logger.LogDebug("Fetching all facilities.");
        var facilities = await _facilitiesRepository.GetAllAsync();
        var facilitiesList = facilities.ToList();
        if (facilitiesList.Count == 0)
        {
            _logger.LogWarning("No facilities found.");
        }

        _logger.LogInformation("Retrieved {FacilityCount} facilities.", facilitiesList.Count);
        return facilitiesList.Select(f => f.ToResult()).ToList();
    }

    public async Task<FacilityResult> AddFacilityAsync(CreateFacilityRequest createRequest)
    {
        var facility = createRequest.ToEntity();
        facility = await _facilitiesRepository.AddAsync(facility);
        _logger.LogInformation("Facility added: {FacilityName} with ID {FacilityId}", facility.Name, facility.Id);
        return facility.ToResult();
    }

    public async Task<bool> UpdateFacilityAsync(int id, UpdateFacilityRequest updateRequest)
    {
        _logger.LogDebug("Updating facility with ID {FacilityId}", id);
        var facility = await _facilitiesRepository.GetByIdAsync(id);
        if (facility == null)
        {
            _logger.LogWarning("Facility with ID {FacilityId} not found.", id);
            return false;
        }

        facility = updateRequest.ToEntity();
        var updated = await _facilitiesRepository.UpdateAsync(facility);
        if (!updated)
        {
            _logger.LogWarning("Failed to update facility with ID {FacilityId}.", id);
        }

        _logger.LogInformation("Facility with ID {FacilityId} updated successfully.", id);
        return updated;
    }

    public bool IsFacilityAvailableForOperations(FacilityResult facilityDto)
    {
        _logger.LogDebug("Checking if facility with ID {FacilityId} is available for operations.", facilityDto.Id);
        return facilityDto.IsActive;
    }

    public async Task<bool> ActivateFacilityAsync(FacilityResult dto)
    {
        _logger.LogDebug("Activating facility with ID {FacilityId}", dto.Id);
        if (dto.IsActive) return true;

        var facility = dto.ToEntity();
        facility.IsActive = true;
        var activated = await _facilitiesRepository.UpdateAsync(facility);
        if (!activated)
        {
            _logger.LogWarning("Failed to activate facility with ID {FacilityId}.", dto.Id);
        }

        _logger.LogInformation("Facility with ID {FacilityId} activated successfully.", dto.Id);
        return activated;
    }

    public async Task<bool> DeactivateFacilityAsync(FacilityResult dto)
    {
        _logger.LogDebug("Deactivating facility with ID {FacilityId}", dto.Id);
        if (!dto.IsActive)
            return true;
        var facility = dto.ToEntity();

        facility.IsActive = false;
        var deactivated = await _facilitiesRepository.UpdateAsync(facility);
        if (!deactivated)
        {
            _logger.LogWarning("Failed to deactivate facility with ID {FacilityId}.", dto.Id);
        }

        _logger.LogInformation("Facility with ID {FacilityId} deactivated successfully.", dto.Id);
        return deactivated;
    }
}
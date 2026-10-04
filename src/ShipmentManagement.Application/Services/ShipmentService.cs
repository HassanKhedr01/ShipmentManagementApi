using Microsoft.Extensions.Logging;

using ShipmentManagement.Application.DTOs.DeliveryAttempts;
using ShipmentManagement.Application.DTOs.Facilities;
using ShipmentManagement.Application.DTOs.TrackingEvents;
using ShipmentManagement.Application.Interfaces;
using ShipmentManagement.Application.Mappings;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Domain.Enums;
using ShipmentManagement.Domain.Repositories;

namespace ShipmentManagement.Application.Services;

public class ShipmentService : IShipmentService
{
    private readonly ILogger<ShipmentService> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITrackingRepository _trackingRepository;
    private readonly IPackagesRepository _packagesRepository;
    private readonly IPackagesService _packagesService;
    private readonly IFacilitiesService _facilitiesService;
    private readonly IAttemptsRepository _attemptsRepository;

    public ShipmentService(ITrackingRepository trackingRepository, IAttemptsRepository attemptsRepository,
        ICurrentUserService currentUserService, IPackagesService packagesService, IFacilitiesService facilitiesService,
        ILogger<ShipmentService> logger, IPackagesRepository packagesRepository)
    {
        _logger = logger;
        _packagesRepository = packagesRepository;
        _trackingRepository = trackingRepository;
        _attemptsRepository = attemptsRepository;
        _currentUserService = currentUserService;
        _packagesService = packagesService;
        _facilitiesService = facilitiesService;
    }

    public async Task<List<TrackingEventResult>> GetEventsByTrackingNumberAsync(string tracingNumber)
    {
        _logger.LogDebug("Fetching tracking events for package with tracking number: {TrackingNumber}", tracingNumber);
        var events = (await _trackingRepository.GetAllByTrackingNumberAsync(tracingNumber)).ToList();
        if (events.Count == 0)
        {
            _logger.LogWarning("No tracking events found for package with tracking number {TrackingNumber}.",
                tracingNumber);
        }

        _logger.LogInformation(
            "Retrieved {EventCount} tracking events for package with tracking number {TrackingNumber}.",
            events.Count, tracingNumber);
        return events.Select(e => e.ToResult()).ToList();
    }

    public async Task<List<TrackingEventResult>> GetEventsByPackageIdAsync(int packageId)
    {
        _logger.LogDebug("Fetching tracking events for package ID: {PackageId}", packageId);
        var events = (await _trackingRepository.GetAllByPackageIdAsync(packageId)).ToList();
        if (events.Count == 0)
        {
            _logger.LogWarning("No tracking events found for package ID {PackageId}.", packageId);
        }

        _logger.LogInformation("Retrieved {EventCount} tracking events for package ID {PackageId}.", events.Count,
            packageId);
        return events.Select(e => e.ToResult()).ToList();
    }

    public async Task<List<TrackingEventResult>> GetUserEventsByPackageIdAsync(int packageId)
    {
        var userId = GetUserId();
        _logger.LogDebug("Fetching tracking events for package ID {PackageId} user ID {UserId}", packageId, userId);

        var events = (await _trackingRepository.GetAllByPackageIdAsync(userId, packageId)).ToList();
        if (events.Count == 0)
        {
            _logger.LogWarning("No tracking events found for package ID {PackageId} and user ID {UserId}.",
                packageId, userId);
        }

        _logger.LogInformation(
            "Retrieved {EventCount} tracking events for package ID {PackageId} and user ID {UserId}.", events.Count,
            packageId, userId);
        return events.Select(e => e.ToResult()).ToList();
    }

    public async Task<List<DeliveryAttemptResult>> GetDeliveryAttemptsByPackageIdAsync(int packageId)
    {
        _logger.LogDebug("Fetching delivery attempts for package ID: {PackageId}", packageId);
        var attempts = (await _attemptsRepository.GetByPackageIdAsync(packageId)).ToList();
        if (attempts.Count == 0)
        {
            _logger.LogWarning("No delivery attempts found for package ID {PackageId}.", packageId);
        }

        _logger.LogInformation("Retrieved {AttemptCount} delivery attempts for package ID {PackageId}.", attempts.Count,
            packageId);
        return attempts.Select(a => a.ToResult()).ToList();
    }

    public async Task<RecordEventResult> RecordEventAsync(int packageId, CreateTrackingEventRequest createRequest)
    {
        _logger.LogDebug("Recording tracking event for package ID: {PackageId}", packageId);

        var package = await _packagesRepository.GetByIdAsync(packageId);
        if (package == null)
        {
            _logger.LogWarning("Package not found for ID: {PackageId}", packageId);
            return new RecordEventResult { IsSuccess = false, Message = "Package not found." };
        }

        if (!_packagesService.CanAcceptTrackingEvents(package))
        {
            _logger.LogWarning("Package ID {PackageId} is not in a state to accept tracking events.", packageId);
            return new RecordEventResult
                { IsSuccess = false, Message = "Package is not in a state to accept tracking events." };
        }

        FacilityResult? facilityResult = null;
        if (createRequest.FacilityId.HasValue)
        {
            facilityResult = await _facilitiesService.GetFacilityByIdAsync(createRequest.FacilityId.Value);

            if (facilityResult == null)
            {
                _logger.LogWarning("Facility not found for ID: {FacilityId}", createRequest.FacilityId);
                return new RecordEventResult { IsSuccess = false, Message = "Facility not found." };
            }

            if (!_facilitiesService.IsFacilityAvailableForOperations(facilityResult))
            {
                _logger.LogWarning("Facility with ID {FacilityId} is not available for operations.", createRequest.FacilityId);
                return new RecordEventResult { IsSuccess = false, Message = "Facility is not available for operations." };
            }
        }

        var currentStatus = package.CurrentStatus;
        var newStatus = createRequest.NewStatus.ToDomain();
        if (!_packagesService.CanTransitionToStatus(currentStatus, newStatus))
        {
            _logger.LogWarning("Invalid status transition for package ID {PackageId}.", packageId);
            return new RecordEventResult { IsSuccess = false, Message = "Invalid status transition." };
        }

        if (newStatus == Status.Delivered)
        {
            var deliveryAttempt = new DeliveryAttempt
            {
                PackageId = packageId,
                AttemptedAt = DateTime.UtcNow,
                DeliveryResult = Result.Successful
            };

            await _attemptsRepository.AddAsync(deliveryAttempt);
            _logger.LogInformation("Successful delivery attempt recorded for package with ID {PackageId}.", packageId);
        }
        else if (newStatus == Status.Delayed)
        {
            var deliveryAttempt = new DeliveryAttempt
            {
                PackageId = packageId,
                AttemptedAt = DateTime.UtcNow,
                DeliveryResult = Result.Failed,
                FailureReason = createRequest.Description
            };

            await _attemptsRepository.AddAsync(deliveryAttempt);
            _logger.LogInformation("Failed delivery attempt recorded for package with ID {PackageId}.", packageId);
        }

        var eventEntity = createRequest.ToEntity();

        eventEntity.PackageId = package.Id;
        eventEntity.PackageName = package.Name;
        eventEntity.FacilityName = newStatus != Status.Delivered ? facilityResult?.Name : null;
        eventEntity.OccuredAt = DateTime.UtcNow;

        var createdEvent = await _trackingRepository.AddAsync(eventEntity);

        package.CurrentStatus = newStatus; //update status

        await _trackingRepository.SaveChangesAsync();
        _logger.LogInformation("Tracking event recorded successfully for package with ID {PackageId}.", packageId);
        _logger.LogInformation("Package status changed for package with ID {PackageId} to {NewStatus}.", packageId,
            newStatus);

        return new RecordEventResult { IsSuccess = true, TrackingEventResult = createdEvent.ToResult() };
    }

    private Guid GetUserId()
    {
        var id = _currentUserService.UserId;
        return id ?? throw new UnauthorizedAccessException("User is not authenticated.");
    }
}
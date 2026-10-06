using Microsoft.Extensions.Logging;

using ShipmentManagement.Application.DTOs.Packages;
using ShipmentManagement.Application.Interfaces;
using ShipmentManagement.Application.Mappings;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Domain.Enums;
using ShipmentManagement.Domain.Repositories;

namespace ShipmentManagement.Application.Services;

public class PackagesService : IPackagesService
{
    private const int EstimatedStandardDeliveryDays = 3;
    private const int EstimatedExpressDeliveryDays = 1;
    private readonly ILogger<PackagesService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPackagesRepository _packagesRepository;

    public PackagesService(IPackagesRepository packagesRepository, ICurrentUserService currentUserService,
        ILogger<PackagesService> logger, TimeProvider timeProvider)
    {
        _packagesRepository = packagesRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<PackageResult?> GetPackageByIdAsync(int id)
    {
        _logger.LogDebug("Fetching package with ID: {PackageId}", id);
        var package = await _packagesRepository.GetByIdAsync(id);
        if (package == null)
        {
            _logger.LogWarning("Package with ID {PackageId} not found", id);
            return null;
        }

        _logger.LogInformation("Package: {PackageName} with ID {PackageId} found", package.Name, package.Id);
        return package.ToResult();
    }

    public async Task<PackageResult?> GetUserPackageByIdAsync(int id)
    {
        var userId = GetUserId();
        _logger.LogDebug("Fetching package with ID {PackageId} for user ID {UserId}", id, userId);
        var package = await _packagesRepository.GetByIdAsync(userId, id);
        if (package == null)
        {
            _logger.LogWarning("Package with ID {PackageId} not found for user ID {UserId}", id, userId);
            return null;
        }

        _logger.LogInformation("Package: {PackageName} with ID {PackageId} found for user ID {UserId}", package.Name,
            package.Id, userId);
        return package.ToResult();
    }

    public async Task<List<PackageResult>> GetAllPackagesAsync()
    {
        _logger.LogDebug("Fetching all packages.");
        var packages = await _packagesRepository.GetAllAsync();
        var packagesList = packages.ToList();
        if (packagesList.Count == 0)
        {
            _logger.LogWarning("No packages found.");
        }

        _logger.LogInformation("Retrieved {PackageCount} packages.", packagesList.Count);
        return packagesList.Select(p => p.ToResult()).ToList();
    }

    public async Task<List<PackageResult>> GetAllUserPackagesAsync()
    {
        var userId = GetUserId();
        _logger.LogDebug("Fetching all packages for user ID {UserId}.", userId);
        var packages = await _packagesRepository.GetAllAsync(userId);
        var packagesList = packages.ToList();
        if (packagesList.Count == 0)
        {
            _logger.LogWarning("No packages found for user ID {UserId}.", userId);
        }

        _logger.LogInformation("Retrieved {PackageCount} packages for user ID {UserId}.", packagesList.Count, userId);
        return packagesList.Select(p => p.ToResult()).ToList();
    }

    public async Task<PackageResult?> GetPackageByTrackingNumberAsync(string trackingNumber)
    {
        _logger.LogDebug("Fetching package by tracking number: {TrackingNumber}", trackingNumber);
        var package = await _packagesRepository.GetByTrackingNumberAsync(trackingNumber);
        if (package == null)
        {
            _logger.LogWarning("Package with tracking number {TrackingNumber} not found.", trackingNumber);
            return null;
        }

        _logger.LogInformation("Package found with tracking number {TrackingNumber}.", trackingNumber);
        return package.ToResult();
    }

    public bool CanAcceptTrackingEvents(Package package)
    {
        _logger.LogDebug(
            "Checking if tracking events can be accepted for package with ID {PackageId} and current status {CurrentStatus}",
            package.Id, package.CurrentStatus);

        return package.CurrentStatus != Status.Delivered &&
               package.CurrentStatus != Status.Cancelled;
    }

    public bool CanTransitionToStatus(Status currentStatus, Status newStatus)
    {
        _logger.LogDebug("Checking if transition from {CurrentStatus} to {NewStatus} is allowed.", currentStatus,
            newStatus);

        return (currentStatus, newStatus) switch
        {
            (Status.Created, Status.PickedUp) => true,
            (Status.Created, Status.Cancelled) => true,
            (Status.PickedUp, Status.InTransit) => true,
            (Status.InTransit, Status.ArrivedAtFacility) => true,
            (Status.InTransit, Status.Delayed) => true,
            (Status.InTransit, Status.OutForDelivery) => true,
            (Status.ArrivedAtFacility, Status.InTransit) => true,
            (Status.Delayed, Status.InTransit) => true,
            (Status.Delayed, Status.OutForDelivery) => true,
            (Status.OutForDelivery, Status.Delayed) => true,
            (Status.OutForDelivery, Status.Delivered) => true,
            _ => false
        };
    }

    public async Task<PackageResult> CreatePackageAsync(CreatePackageRequest createRequest)
    {
        var package = createRequest.ToEntity();

        package.CurrentStatus = Status.Created;
        var currentUtc = _timeProvider.GetUtcNow().UtcDateTime;
        package.EstimatedDeliveryDate = currentUtc.AddDays(package.DeliveryType == DeliveryType.Express
            ? EstimatedExpressDeliveryDays
            : EstimatedStandardDeliveryDays);
        package.TrackingEvents.Add(new TrackingEvent
        {
            PackageId = package.Id,
            PackageName = package.Name,
            Status = Status.Created,
            OccuredAt = currentUtc,
            Description = "New package created",
            Package = package
        });

        var userId = GetUserId();
        package.ApplicationUserId = userId;
        var addedPackage = await _packagesRepository.AddAsync(package);
        _logger.LogInformation("Created Package with name {PackageName} successfully", addedPackage.Name);
        return addedPackage.ToResult();
    }

    public async Task<bool> CancelPackageAsync(int id)
    {
        var userId = GetUserId();
        _logger.LogDebug("Cancelling package with ID {PackageId} for User ID {UserId}", id, userId);
        var package = await _packagesRepository.GetByIdAsync(userId, id);
        if (package == null)
        {
            _logger.LogWarning("Package with ID {PackageId} not found.", id);
            return false;
        }

        if (package.CurrentStatus != Status.Created)
        {
            _logger.LogWarning("Package with ID {PackageId} is not in a cancellable state.", id);
            return false;
        }

        package.CurrentStatus = Status.Cancelled;
        package.TrackingEvents.Add(new TrackingEvent
        {
            PackageId = package.Id,
            PackageName = package.Name,
            Status = Status.Cancelled,
            OccuredAt = _timeProvider.GetUtcNow().UtcDateTime,
            Description = "Customer cancelled ordering the package",
            Package = package
        });
        return await _packagesRepository.UpdateAsync(package);
    }

    private Guid GetUserId()
    {
        var id = _currentUserService.UserId;
        return id ?? throw new UnauthorizedAccessException("User is not authenticated");
    }
}
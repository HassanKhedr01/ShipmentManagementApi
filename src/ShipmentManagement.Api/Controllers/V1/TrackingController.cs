using Microsoft.AspNetCore.Mvc;
using ShipmentManagement.Api.Models;
using ShipmentManagement.Application.Interfaces;

namespace ShipmentManagement.Api.Controllers.V1;

/// <summary>
/// Controller for tracking packages and retrieving their events.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class TrackingController : ControllerBase
{
    private readonly IPackagesService _packagesService;
    private readonly IShipmentService _shipmentService;

    public TrackingController(IShipmentService shipmentService, IPackagesService packagesService)
    {
        _shipmentService = shipmentService;
        _packagesService = packagesService;
    }

    /// <summary>
    /// Get information about a package by its tracking number.
    /// </summary>
    /// <param name="trackingNumber">The tracking number of the package.</param>
    /// <returns>The package information or a not found response.</returns>
    /// <response code="200">The package information was successfully retrieved.</response>
    /// <response code="400">The tracking number is invalid.</response>
    /// <response code="404">The package was not found.</response>
    [HttpGet("{trackingNumber?}")]
    [ProducesResponseType(typeof(PublicPackageInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicPackageInfo>> GetPackage(string? trackingNumber)
    {
        if (string.IsNullOrEmpty(trackingNumber) || trackingNumber.Length != 12)
        {
            return BadRequest(new { Message = "Enter a valid tracking number" });
        }

        var package = await _packagesService.GetPackageByTrackingNumberAsync(trackingNumber);
        if (package == null)
        {
            return NotFound(new { Message = "Package not found" });
        }

        var publicPackage = new PublicPackageInfo
        {
            TrackingNumber = package.TrackingNumber,
            CurrentStatus = package.CurrentStatus,
            EstimatedDeliveryDate = package.EstimatedDeliveryDate,
            CreatedAt = package.CreatedAt
        };

        return Ok(publicPackage);
    }

    /// <summary>
    /// Get the tracking events for a package by its tracking number.
    /// </summary>
    /// <param name="trackingNumber">The tracking number of the package.</param>
    /// <returns>The list of tracking events or a not found response.</returns>
    /// <response code="200">The tracking events were successfully retrieved.</response>
    /// <response code="400">The tracking number is invalid.</response>
    /// <response code="404">The package was not found.</response>
    [HttpGet("{trackingNumber}/events")]
    [ProducesResponseType(typeof(List<PublicTrackingEvent>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PublicTrackingEvent>>> GetPackageEvents(string trackingNumber)
    {
        if (string.IsNullOrEmpty(trackingNumber) || trackingNumber.Length != 12)
        {
            return BadRequest(new { Message = "Enter a valid tracking number" });
        }

        var events = await _shipmentService.GetEventsByTrackingNumberAsync(trackingNumber);
        if (events.Count == 0)
        {
            return NotFound(new { Message = "No events found for this package" });
        }

        var publicEvents = events.Select(e => new PublicTrackingEvent
        {
            Status = e.Status,
            OccuredAt = e.OccuredAt,
            Description = e.Description
        }).ToList();

        return Ok(publicEvents);
    }
}
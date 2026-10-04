using Microsoft.EntityFrameworkCore;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Domain.Repositories;
using ShipmentManagement.Infrastructure.Persistence;

namespace ShipmentManagement.Infrastructure.Repositories;

public class TrackingRepository : ITrackingRepository
{
    private readonly AppDbContext _context;

    public TrackingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TrackingEvent>> GetAllByTrackingNumberAsync(string trackingNumber)
    {
        return await _context.TrackingEvents.AsNoTracking()
            .Where(te => te.Package.TrackingNumber == trackingNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrackingEvent>> GetAllByPackageIdAsync(int packageId)
    {
        return await _context.TrackingEvents.AsNoTracking()
            .Where(te => te.PackageId == packageId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrackingEvent>> GetAllByPackageIdAsync(Guid userId, int packageId)
    {
        return await _context.TrackingEvents.AsNoTracking()
            .Where(te => te.PackageId == packageId && te.Package.ApplicationUserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TrackingEvent>> GetAllByFacilityIdAsync(int facilityId)
    {
        return await _context.TrackingEvents.AsNoTracking()
            .Where(te => te.FacilityId == facilityId)
            .ToListAsync();
    }

    public async Task<TrackingEvent> AddAsync(TrackingEvent trackingEvent)
    {
        await _context.TrackingEvents.AddAsync(trackingEvent);
        return trackingEvent;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
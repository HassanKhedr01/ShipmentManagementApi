using ShipmentManagement.Domain.Entities;

namespace ShipmentManagement.Domain.Repositories;

public interface ITrackingRepository
{
    Task<IEnumerable<TrackingEvent>> GetAllByTrackingNumberAsync(string trackingNumber);
    Task<IEnumerable<TrackingEvent>> GetAllByPackageIdAsync(int packageId);
    Task<IEnumerable<TrackingEvent>> GetAllByPackageIdAsync(Guid userId, int packageId);
    Task<IEnumerable<TrackingEvent>> GetAllByFacilityIdAsync(int facilityId);
    Task<TrackingEvent> AddAsync(TrackingEvent trackingEvent);
    Task SaveChangesAsync();
}
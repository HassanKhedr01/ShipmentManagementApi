using ShipmentManagement.Domain.Entities;

namespace ShipmentManagement.Domain.Repositories;

public interface IAttemptsRepository
{
    Task<IEnumerable<DeliveryAttempt>> GetByPackageIdAsync(int packageId);
    Task<DeliveryAttempt> AddAsync(DeliveryAttempt deliveryAttempt);
    Task SaveChangesAsync();
}
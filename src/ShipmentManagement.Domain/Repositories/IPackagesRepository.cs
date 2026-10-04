using ShipmentManagement.Domain.Entities;

namespace ShipmentManagement.Domain.Repositories;

public interface IPackagesRepository
{
    Task<Package?> GetByIdAsync(int id);
    Task<Package?> GetByIdAsync(Guid userId, int id);
    Task<IEnumerable<Package>> GetAllAsync(); 
    Task<IEnumerable<Package>> GetAllAsync(Guid userId);
    Task<Package?> GetByTrackingNumberAsync(string trackingNumber);
    Task<Package> AddAsync(Package package);
    Task<bool> UpdateAsync(Package package);
}
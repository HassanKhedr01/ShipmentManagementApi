using ShipmentManagement.Domain.Entities;

namespace ShipmentManagement.Domain.Repositories;

public interface IFacilitiesRepository
{
    Task<Facility?> GetByIdAsync(int? id);
    Task<IEnumerable<Facility>> GetAllAsync();
    Task<Facility> AddAsync(Facility facility);
    Task<bool> UpdateAsync(Facility facility);
}
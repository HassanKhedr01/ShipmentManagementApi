using Microsoft.EntityFrameworkCore;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Domain.Repositories;
using ShipmentManagement.Infrastructure.Persistence;

namespace ShipmentManagement.Infrastructure.Repositories;

public class AttemptsRepository : IAttemptsRepository
{
    private readonly AppDbContext _context;

    public AttemptsRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<DeliveryAttempt>> GetByPackageIdAsync(int packageId)
    {
        return await _context.DeliveryAttempts.AsNoTracking().Where(a => a.PackageId == packageId).ToListAsync();
    }

    public async Task<DeliveryAttempt> AddAsync(DeliveryAttempt deliveryAttempt)
    {
        await _context.DeliveryAttempts.AddAsync(deliveryAttempt);
        return deliveryAttempt;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
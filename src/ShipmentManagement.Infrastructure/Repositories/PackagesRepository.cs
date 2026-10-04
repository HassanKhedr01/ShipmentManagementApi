using Microsoft.EntityFrameworkCore;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Domain.Repositories;
using ShipmentManagement.Infrastructure.Persistence;

namespace ShipmentManagement.Infrastructure.Repositories;

public class PackagesRepository : IPackagesRepository
{
    private readonly AppDbContext _context;

    public PackagesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Package?> GetByIdAsync(int id)
    {
        return await _context.Packages.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Package?> GetByIdAsync(Guid userId, int id)
    {
        return await _context.Packages.FirstOrDefaultAsync(p => p.Id == id && p.ApplicationUserId == userId);
    }

    public async Task<IEnumerable<Package>> GetAllAsync()
    {
        return await _context.Packages.AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Package>> GetAllAsync(Guid userId)
    {
        return await _context.Packages.AsNoTracking().Where(p => p.ApplicationUserId == userId).ToListAsync();
    }

    public async Task<Package?> GetByTrackingNumberAsync(string trackingNumber)
    {
        return await _context.Packages.FirstOrDefaultAsync(p => p.TrackingNumber == trackingNumber);
    }

    public async Task<Package> AddAsync(Package package)
    {
        var trackingNumber = await GenerateTrackingNumber();
        package.TrackingNumber = trackingNumber;
        _context.Packages.Add(package);
        await _context.SaveChangesAsync();
        return package;
    }

    public async Task<bool> UpdateAsync(Package package)
    {
        _context.Packages.Update(package);
        return await _context.SaveChangesAsync() > 0;
    }
    
    private async Task<string> GenerateTrackingNumber()
    {
         var trackingNumber = $"PKG-{Random.Shared.GetHexString(8)}";
         if (await _context.Packages.FirstOrDefaultAsync(p => p.TrackingNumber == trackingNumber) != null)
         {
             trackingNumber = await GenerateTrackingNumber();
         }
         return trackingNumber;
    }
}
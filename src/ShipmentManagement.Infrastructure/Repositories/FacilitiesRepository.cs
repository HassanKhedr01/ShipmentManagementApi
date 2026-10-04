using Microsoft.EntityFrameworkCore;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Domain.Repositories;
using ShipmentManagement.Infrastructure.Persistence;

namespace ShipmentManagement.Infrastructure.Repositories;

public class FacilitiesRepository : IFacilitiesRepository
{
    private readonly AppDbContext _context;

    public FacilitiesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Facility?> GetByIdAsync(int? id)
    {
        return await _context.Facilities.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<Facility>> GetAllAsync()
    {
        return await _context.Facilities.AsNoTracking().ToListAsync();
    }

    public async Task<Facility> AddAsync(Facility facility)
    {
        await _context.Facilities.AddAsync(facility);
        await _context.SaveChangesAsync();
        return facility;
    }

    public async Task<bool> UpdateAsync(Facility facility)
    {
        _context.Facilities.Update(facility);
        return await _context.SaveChangesAsync() > 0;
    }
}
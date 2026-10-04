using Microsoft.EntityFrameworkCore;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Domain.Repositories;
using ShipmentManagement.Infrastructure.Persistence;

namespace ShipmentManagement.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(Guid userId, string tokenHash)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(rt =>
            rt.ApplicationUserId == userId && rt.TokenHash == tokenHash);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> RevokeRefreshTokenAsync(RefreshToken refreshToken)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }
}
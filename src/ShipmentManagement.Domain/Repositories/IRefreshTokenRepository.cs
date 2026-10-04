using ShipmentManagement.Domain.Entities;

namespace ShipmentManagement.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash);
    Task<RefreshToken?> GetRefreshTokenAsync(Guid userId,string tokenHash);
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task<bool> RevokeRefreshTokenAsync(RefreshToken refreshToken);
}
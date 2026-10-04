using System.Security.Cryptography;
using System.Text;

using ShipmentManagement.Application.Interfaces;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Domain.Repositories;

namespace ShipmentManagement.Application.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository)
    {
        _refreshTokenRepository = refreshTokenRepository;
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(bytes);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash)
    {
        return await _refreshTokenRepository.GetRefreshTokenAsync(tokenHash);
    }

    public async Task<RefreshToken?> GetRefreshTokenWithUserIdAsync(Guid userId, string tokenHash)
    {
        return await _refreshTokenRepository.GetRefreshTokenAsync(userId, tokenHash);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _refreshTokenRepository.AddRefreshTokenAsync(refreshToken);
    }

    public async Task<bool> RevokeRefreshTokenAsync(RefreshToken refreshToken)
    {
        return await _refreshTokenRepository.RevokeRefreshTokenAsync(refreshToken);
    }
}
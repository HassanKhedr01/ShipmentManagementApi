using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ShipmentManagement.Application.DTOs.Auth;
using ShipmentManagement.Application.Interfaces;
using ShipmentManagement.Domain.Entities;
using ShipmentManagement.Infrastructure.Identity;

namespace ShipmentManagement.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly TimeProvider _timeProvider;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(UserManager<ApplicationUser> userManager, IRefreshTokenService refreshTokenService,
        IJwtService jwtService, IOptions<JwtOptions> jwtOptions, RoleManager<ApplicationRole> roleManager, TimeProvider timeProvider)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _timeProvider = timeProvider;
        _refreshTokenService = refreshTokenService;
        _jwtService = jwtService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user != null) return new AuthResultDto { Success = false, Errors = ["Invalid Email or Password."] };

        user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email
        };

        var registerResult = await _userManager.CreateAsync(user, dto.Password);
        if (!registerResult.Succeeded)
        {
            return new AuthResultDto { Success = false, Errors = registerResult.Errors.Select(e => e.Description) };
        }

        await EnsureRolesCreated();
        var roleResult = await _userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
        {
            return new AuthResultDto { Success = false, Errors = roleResult.Errors.Select(e => e.Description) };
        }

        return new AuthResultDto { Success = true };
    }

    public async Task<TokenResponseDto?> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) return null;

        var passwordResult = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordResult) return null;

        var userRoles = await _userManager.GetRolesAsync(user);
        var jwtUser = new JwtUserDto
        {
            Id = user.Id,
            Name = user.UserName!,
            Email = user.Email!,
            Roles = userRoles
        };

        var accessToken = _jwtService.GenerateAccessToken(jwtUser);
        var refreshToken = _refreshTokenService.GenerateRefreshToken();
        var refreshTokenHash = _refreshTokenService.HashRefreshToken(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            ApplicationUserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(_jwtOptions.RefreshTokenExpirationDays),
        };

        await _refreshTokenService.AddRefreshTokenAsync(refreshTokenEntity);
        return new TokenResponseDto
        {
            AccessToken = accessToken.Token,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessToken.ExpiresAt
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var refreshTokenHash = _refreshTokenService.HashRefreshToken(refreshToken);
        var storedToken = await _refreshTokenService.GetRefreshTokenAsync(refreshTokenHash);
        if (storedToken == null) return false;
        if (storedToken.RevokedAt is not null) return false;

        await _refreshTokenService.RevokeRefreshTokenAsync(storedToken);
        return true;
    }

    public async Task<TokenResponseDto?> RefreshAsync(RefreshRequestDto dto)
    {
        var principle = _jwtService.GetPrincipleFromExpiredToken(dto.ExpiredAccessToken);
        if (principle == null) return null;

        var userIdString = principle.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdString, out var userId)) return null;
        
        var tokenHash = _refreshTokenService.HashRefreshToken(dto.RefreshToken);
        var storedToken = await _refreshTokenService.GetRefreshTokenWithUserIdAsync(userId,tokenHash);

        var currentUtc = _timeProvider.GetUtcNow().UtcDateTime;
        if (storedToken is not { RevokedAt: null } || storedToken.ExpiresAt <= currentUtc)
        {
            return null;
        }

        await _refreshTokenService.RevokeRefreshTokenAsync(storedToken);

        var user = await _userManager.FindByIdAsync(storedToken.ApplicationUserId.ToString());
        if (user == null) return null;

        var userRoles = await _userManager.GetRolesAsync(user);
        var jwtUser = new JwtUserDto
        {
            Id = user.Id,
            Name = user.UserName!,
            Email = user.Email!,
            Roles = userRoles
        };

        var accessToken = _jwtService.GenerateAccessToken(jwtUser);
        var newRefreshToken = _refreshTokenService.GenerateRefreshToken();
        var refreshTokenHash = _refreshTokenService.HashRefreshToken(newRefreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            ApplicationUserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = currentUtc.AddDays(_jwtOptions.RefreshTokenExpirationDays),
        };

        await _refreshTokenService.AddRefreshTokenAsync(refreshTokenEntity);
        return new TokenResponseDto
        {
            AccessToken = accessToken.Token,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = accessToken.ExpiresAt
        };
    }

    private async Task EnsureRolesCreated()
    {
        var roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new ApplicationRole(role));
            }
        }
    }
}
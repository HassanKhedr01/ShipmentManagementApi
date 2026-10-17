using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShipmentManagement.Api.Filters;
using ShipmentManagement.Application.DTOs.Auth;
using ShipmentManagement.Application.Interfaces;

namespace ShipmentManagement.Api.Controllers.V1;

/// <summary>
/// Controller for handling authentication-related operations such as registration, login, token refresh, and logout.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly IValidator<LoginRequestDto> _loginValidator;
    private readonly IValidator<RefreshRequestDto> _refreshValidator;
    private readonly IValidator<LogoutRequestDto> _logoutValidator;
    private readonly IAuthService _authService;

    public AuthController(IValidator<RegisterRequestDto> registerValidator, IValidator<LoginRequestDto> loginValidator,
        IValidator<RefreshRequestDto> refreshValidator, IValidator<LogoutRequestDto> logoutValidator,
        IAuthService authService)
    {
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
        _logoutValidator = logoutValidator;
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user with the provided registration details.
    /// </summary>
    /// <param name="requestDto">The registration request details</param>
    /// <returns></returns>
    /// <response code="200">User registered successfully</response>
    /// <response code="400">Invalid registration request</response>
    [AnonymousOnly]
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto requestDto)
    {
        var validationResult = await _registerValidator.ValidateAsync(requestDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var result = await _authService.RegisterAsync(requestDto);

        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }

        return Ok(new { Message = "User registered successfully." });
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token if the credentials are valid.
    /// </summary>
    /// <param name="requestDto">The login request details</param>
    /// <returns>The JWT token if authentication is successful</returns>
    /// <response code="200">Authentication successful, returns JWT token</response>
    /// <response code="401">Authentication failed</response>
    /// <response code="400">Invalid login request</response>
    [AnonymousOnly]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginRequestDto requestDto)
    {
        var validationResult = await _loginValidator.ValidateAsync(requestDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var token = await _authService.LoginAsync(requestDto);
        if (token == null)
        {
            return Unauthorized();
        }

        return Ok(token);
    }

    /// <summary>
    /// Refreshes the JWT token using a valid refresh token.
    /// </summary>
    /// <param name="requestDto">The refresh request details</param>
    /// <returns>The new JWT token if the refresh is successful</returns>
    /// <response code="200">Refresh successful, returns new JWT token</response>
    /// <response code="401">Refresh failed</response>
    /// <response code="400">Invalid refresh request</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenResponseDto>> Refresh([FromBody] RefreshRequestDto requestDto)
    {
        var validationResult = await _refreshValidator.ValidateAsync(requestDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var token = await _authService.RefreshAsync(requestDto);
        if (token == null)
        {
            return Unauthorized();
        }

        return Ok(token);
    }

    /// <summary>
    /// Logs out the current user.
    /// </summary>
    /// <param name="requestDto">The logout request details</param>
    /// <returns></returns>
    /// <response code="204">Logout successful</response>
    /// <response code="400">Invalid logout request</response>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto requestDto)
    {
        var validationResult = await _logoutValidator.ValidateAsync(requestDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }

        var loggedOut = await _authService.LogoutAsync(requestDto.RefreshToken);
        if (!loggedOut)
        {
            return BadRequest(new { Message = "Failed to log out." });
        }

        return NoContent();
    }
}
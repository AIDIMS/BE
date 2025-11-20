using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIDIMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<LoginDto> _loginValidator;
    private readonly IValidator<ChangePasswordDto> _changePasswordValidator;

    public AuthController(
        IAuthService authService,
        IValidator<LoginDto> loginValidator,
        IValidator<ChangePasswordDto> changePasswordValidator)
    {
        _authService = authService;
        _loginValidator = loginValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<Result<AuthResponseDto>>> Login(
        [FromBody] LoginDto loginDto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _loginValidator.ValidateAsync(loginDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<AuthResponseDto>.Failure("Validation failed", errors));
        }

        var result = await _authService.LoginAsync(loginDto, cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(result);
        }

        // Set refresh token in HttpOnly cookie
        if (result.Data != null)
        {
            SetRefreshTokenCookie(result.Data.RefreshToken);
            result.Data.RefreshToken = "";
        }

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<Result<AuthResponseDto>>> RefreshToken(CancellationToken cancellationToken)
    {
        // Try to get refresh token from body or cookie
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest(Result<AuthResponseDto>.Failure("Refresh token is required"));
        }

        var result = await _authService.RefreshTokenAsync(refreshToken, cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(result);
        }

        // Set new refresh token in HttpOnly cookie
        if (result.Data != null)
        {
            SetRefreshTokenCookie(result.Data.RefreshToken);
            result.Data.RefreshToken = "";
        }

        return Ok(result);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<Result<bool>>> ChangePassword(
        [FromBody] ChangePasswordDto changePasswordDto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _changePasswordValidator.ValidateAsync(changePasswordDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<bool>.Failure("Validation failed", errors));
        }

        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(Result<bool>.Failure("Invalid token"));
        }

        var result = await _authService.ChangePasswordAsync(userId, changePasswordDto, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value;
        var lastName = User.FindFirst(ClaimTypes.Surname)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var department = User.FindFirst("Department")?.Value;

        return Ok(new
        {
            UserId = userId,
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            Department = department
        });
    }

    [HttpPost("validate-token")]
    [AllowAnonymous]
    public async Task<ActionResult<Result<bool>>> ValidateToken(
        [FromBody] string token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(Result<bool>.Failure("Token is required"));
        }

        var result = await _authService.ValidateTokenAsync(token, cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<Result<bool>>> Logout(CancellationToken cancellationToken)
    {
        // Get user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(Result<bool>.Failure("Invalid token"));
        }

        // Revoke all refresh tokens from database
        var result = await _authService.RevokeRefreshTokensAsync(userId, cancellationToken);

        // Clear refresh token cookie
        ClearRefreshTokenCookie();

        return Ok(result);
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Set to true in production with HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7).AddHours(7)
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete("refreshToken");
    }
}

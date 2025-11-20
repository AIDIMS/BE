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

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<Result<AuthResponseDto>>> RefreshToken(
        [FromBody] RefreshTokenDto refreshTokenDto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(refreshTokenDto.RefreshToken))
        {
            return BadRequest(Result<AuthResponseDto>.Failure("Refresh token is required"));
        }

        var result = await _authService.RefreshTokenAsync(refreshTokenDto.RefreshToken, cancellationToken);

        if (!result.IsSuccess)
        {
            return Unauthorized(result);
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
}

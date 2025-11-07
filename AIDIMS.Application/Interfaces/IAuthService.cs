using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;

namespace AIDIMS.Application.Interfaces;

/// <summary>
/// Interface for authentication service
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticate user and return JWT tokens
    /// </summary>
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Register a new user
    /// </summary>
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Change user password
    /// </summary>
    Task<Result<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate token
    /// </summary>
    Task<Result<bool>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}

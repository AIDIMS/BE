using System.Security.Claims;
using AIDIMS.Domain.Entities;

namespace AIDIMS.Application.Interfaces;

/// <summary>
/// Interface for JWT token service
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generate access token for user
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generate refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validate token and return claims principal
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Get user ID from token
    /// </summary>
    Guid? GetUserIdFromToken(string token);
}

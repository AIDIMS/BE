using System.Security.Claims;
using AIDIMS.Domain.Entities;

namespace AIDIMS.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);

    string GenerateRefreshToken();

    ClaimsPrincipal? ValidateToken(string token);

    Guid? GetUserIdFromToken(string token);
}

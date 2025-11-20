using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;

namespace AIDIMS.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);

    Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<Result<bool>> RevokeRefreshTokensAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default);

    Task<Result<bool>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}

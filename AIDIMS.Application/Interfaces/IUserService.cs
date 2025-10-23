using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;

namespace AIDIMS.Application.Interfaces;

/// <summary>
/// Interface for User service
/// </summary>
public interface IUserService
{
    Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<UserDto>>> GetAllAsync(PaginationParams paginationParams, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

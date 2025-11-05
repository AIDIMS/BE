using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Interfaces;
using AutoMapper;

namespace AIDIMS.Application.UseCases;

/// <summary>
/// User service implementation
/// </summary>
public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure($"User with ID {id} not found");
        }

        var userDto = _mapper.Map<UserDto>(user);
        return Result<UserDto>.Success(userDto);
    }

    public async Task<Result<PagedResult<UserDto>>> GetAllAsync(
        PaginationParams paginationParams,
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var userList = users.ToList();

        var pagedUsers = userList
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();

        var pagedUserDtos = _mapper.Map<List<UserDto>>(pagedUsers);

        var pagedResult = new PagedResult<UserDto>
        {
            Items = pagedUserDtos,
            PageNumber = paginationParams.PageNumber,
            PageSize = paginationParams.PageSize,
            TotalCount = userList.Count
        };

        return Result<PagedResult<UserDto>>.Success(pagedResult);
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = _mapper.Map<User>(dto);

        // TODO: Add password hashing using a proper password hasher service
        // For now, storing as-is (this should be changed in production)

        user.CreatedAt = DateTime.UtcNow;
        user.Id = Guid.NewGuid();

        var createdUser = await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(createdUser);
        return Result<UserDto>.Success(userDto, "User created successfully");
    }

    public async Task<Result<UserDto>> UpdateAsync(
        Guid id,
        UpdateUserDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure($"User with ID {id} not found");
        }

        _mapper.Map(dto, user);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        return Result<UserDto>.Success(userDto, "User updated successfully");
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return Result.Failure($"User with ID {id} not found");
        }

        await _userRepository.DeleteAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("User deleted successfully");
    }
}

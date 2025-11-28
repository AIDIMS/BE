using System.Security.Claims;
using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Interfaces;
using AutoMapper;

namespace AIDIMS.Application.UseCases;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
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
        SearchUserDto filters,
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllIncludingDeletedAsync(cancellationToken);
        var query = users.AsEnumerable();

        if (filters.Role.HasValue)
        {
            query = query.Where(u => u.Role == filters.Role.Value);
        }

        if (filters.Department.HasValue)
        {
            query = query.Where(u => u.Department == filters.Department.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.FullName))
        {
            var name = filters.FullName.Trim().ToLower();
            query = query.Where(u => !string.IsNullOrEmpty(u.FullName) 
                                     && u.FullName.ToLower().Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(filters.PhoneNumber))
        {
            var phone = filters.PhoneNumber.Trim();
            query = query.Where(u => !string.IsNullOrEmpty(u.PhoneNumber) 
                                     && u.PhoneNumber.Contains(phone));
        }
        
        var userList = query.ToList();

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

        user.PasswordHash = _passwordHasher.HashPassword(dto.Password);
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

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        return Result<UserDto>.Success(userDto, "User updated successfully");
    }

    public async Task<Result<UserDto>> UpdateByIdentifyAsync(
        Guid id,
        UpdateUserByIdentifyDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
        {
            return Result<UserDto>.Failure($"User with ID {id} not found");
        }

        _mapper.Map(dto, user);

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

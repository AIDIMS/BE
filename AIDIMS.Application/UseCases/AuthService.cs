using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace AIDIMS.Application.UseCases;

/// <summary>
/// Authentication service implementation
/// </summary>
public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(
        IRepository<User> userRepository,
        IRepository<RefreshToken> refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IMapper mapper,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        // Find user by username
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.Username == loginDto.Username);

        if (user == null)
        {
            return Result<AuthResponseDto>.Failure("Invalid username or password");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Failure("Invalid username or password");
        }

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7")),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "60")),
            User = userDto
        };

        return Result<AuthResponseDto>.Success(response, "Login successful");
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        // Check if username already exists
        var users = await _userRepository.GetAllAsync(cancellationToken);
        if (users.Any(u => u.Username == registerDto.Username))
        {
            return Result<AuthResponseDto>.Failure("Username already exists");
        }

        // Check if email already exists
        if (!string.IsNullOrEmpty(registerDto.Email) && users.Any(u => u.Email == registerDto.Email))
        {
            return Result<AuthResponseDto>.Failure("Email already exists");
        }

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = registerDto.Username,
            PasswordHash = _passwordHasher.HashPassword(registerDto.Password),
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PhoneNumber = registerDto.PhoneNumber,
            Role = registerDto.Role,
            Department = registerDto.Department,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7")),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "60")),
            User = userDto
        };

        return Result<AuthResponseDto>.Success(response, "Registration successful");
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        // Find refresh token
        var refreshTokens = await _refreshTokenRepository.GetAllAsync(cancellationToken);
        var storedToken = refreshTokens.FirstOrDefault(rt => rt.Token == refreshToken);

        if (storedToken == null)
        {
            return Result<AuthResponseDto>.Failure("Invalid refresh token");
        }

        if (!storedToken.IsActive)
        {
            return Result<AuthResponseDto>.Failure("Refresh token is expired or revoked");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
        if (user == null)
        {
            return Result<AuthResponseDto>.Failure("User not found");
        }

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Revoke old refresh token
        storedToken.IsRevoked = true;
        storedToken.ReplacedByToken = newRefreshToken;
        await _refreshTokenRepository.UpdateAsync(storedToken, cancellationToken);

        // Save new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7")),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = _mapper.Map<UserDto>(user);
        var response = new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "60")),
            User = userDto
        };

        return Result<AuthResponseDto>.Success(response, "Token refreshed successfully");
    }

    public async Task<Result<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return Result<bool>.Failure("User not found");
        }

        // Verify current password
        if (!_passwordHasher.VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            return Result<bool>.Failure("Current password is incorrect");
        }

        // Update password
        user.PasswordHash = _passwordHasher.HashPassword(changePasswordDto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true, "Password changed successfully");
    }

    public async Task<Result<bool>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var principal = _jwtService.ValidateToken(token);
        if (principal == null)
        {
            return Result<bool>.Failure("Invalid token");
        }

        return Result<bool>.Success(true, "Token is valid");
    }
}

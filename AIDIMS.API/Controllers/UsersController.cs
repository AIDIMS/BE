using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AIDIMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<CreateUserDto> _createUserValidator;
    private readonly IValidator<UpdateUserDto> _updateUserValidator;

    public UsersController(
        IUserService userService,
        IValidator<CreateUserDto> createUserValidator,
        IValidator<UpdateUserDto> updateUserValidator)
    {
        _userService = userService;
        _createUserValidator = createUserValidator;
        _updateUserValidator = updateUserValidator;
    }

    /// <summary>
    /// Get all users with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<UserDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _userService.GetAllAsync(paginationParams, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Result<UserDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Result<UserDto>>> Create(
        [FromBody] CreateUserDto dto,
        CancellationToken cancellationToken)
    {
        // Validate input
        var validationResult = await _createUserValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<UserDto>.Failure("Validation failed", errors));
        }

        var result = await _userService.CreateAsync(dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            result);
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Result<UserDto>>> Update(
        Guid id,
        [FromBody] UpdateUserDto dto,
        CancellationToken cancellationToken)
    {
        // Validate input
        var validationResult = await _updateUserValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<UserDto>.Failure("Validation failed", errors));
        }

        var result = await _userService.UpdateAsync(id, dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _userService.DeleteAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}

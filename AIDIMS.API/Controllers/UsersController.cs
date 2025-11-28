using AIDIMS.API.Filters;
using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIDIMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<CreateUserDto> _createUserValidator;
    private readonly IValidator<UpdateUserDto> _updateUserValidator;
    private readonly IValidator<UpdateUserByIdentifyDto> _updateUserByIdentifyValidator;

    public UsersController(
        IUserService userService,
        IValidator<CreateUserDto> createUserValidator,
        IValidator<UpdateUserDto> updateUserValidator,
        IValidator<UpdateUserByIdentifyDto> updateUserByIdentifyValidator)
    {
        _userService = userService;
        _createUserValidator = createUserValidator;
        _updateUserValidator = updateUserValidator;
        _updateUserByIdentifyValidator = updateUserByIdentifyValidator;
    }

    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<UserDto>>>> GetAll(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] SearchUserDto filters,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetAllAsync(paginationParams, filters, cancellationToken);
        return Ok(result);
    }

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

    [HttpPost]
    [AdminOnly]
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

    [HttpPut("{id}")]
    [AdminOnly]
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

    [HttpPut("identify/{id}")]
    public async Task<ActionResult<Result<UserDto>>> UpdateByIdentify(
        Guid id,
        [FromBody] UpdateUserByIdentifyDto dto,
        CancellationToken cancellationToken)
    {
        // Validate input
        var validationResult = await _updateUserByIdentifyValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<UserDto>.Failure("Validation failed", errors));
        }

        var result = await _userService.UpdateByIdentifyAsync(id, dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [AdminOnly]
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

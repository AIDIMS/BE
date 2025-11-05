using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AIDIMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IValidator<CreatePatientDto> _createPatientValidator;
    private readonly IValidator<UpdatePatientDto> _updatePatientValidator;

    public PatientsController(
        IPatientService patientService,
        IValidator<CreatePatientDto> createPatientValidator,
        IValidator<UpdatePatientDto> updatePatientValidator)
    {
        _patientService = patientService;
        _createPatientValidator = createPatientValidator;
        _updatePatientValidator = updatePatientValidator;
    }

    /// <summary>
    /// Get all patients with pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<PatientDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? fullName = null,
        [FromQuery] DateTime? dateOfBirth = null,
        [FromQuery] string? gender = null,
        [FromQuery] string? phoneNumber = null,
        CancellationToken cancellationToken = default)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var filters = new SearchPatientDto
        {
            FullName = fullName,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            PhoneNumber = phoneNumber
        };

        var result = await _patientService.GetAllAsync(paginationParams, filters, cancellationToken);
        return Ok(result);
    }

/// <summary>
    /// Get patient by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Result<PatientDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _patientService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new patient
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Result<PatientDto>>> Create(
        [FromBody] CreatePatientDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createPatientValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<UserDto>.Failure("Validation failed", errors));
        }

        var result = await _patientService.CreateAsync(dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update an existing patient
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Result<PatientDto>>> Update(
        Guid id,
        [FromBody] UpdatePatientDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updatePatientValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<PatientDto>.Failure("Validation failed", errors));
        }

        var result = await _patientService.UpdateAsync(id, dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);

    }

    /// <summary>
    /// Delete a patient by ID
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<Result>> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _patientService.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}

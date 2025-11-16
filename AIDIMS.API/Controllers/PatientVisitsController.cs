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
public class PatientVisitsController : ControllerBase
{
    private readonly IPatientVisitService _patientVisitService;
    private readonly IValidator<CreatePatientVisitDto> _createValidator;
    private readonly IValidator<UpdatePatientVisitDto> _updateValidator;

    public PatientVisitsController(
        IPatientVisitService patientVisitService,
        IValidator<CreatePatientVisitDto> createValidator,
        IValidator<UpdatePatientVisitDto> updateValidator)
    {
        _patientVisitService = patientVisitService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Get all patient visits with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<PatientVisitDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? patientId = null,
        [FromQuery] Guid? assignedDoctorId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var filters = new SearchPatientVisitDto
        {
            PatientId = patientId,
            AssignedDoctorId = assignedDoctorId,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _patientVisitService.GetAllAsync(paginationParams, filters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get patient visit by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Result<PatientVisitDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _patientVisitService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new patient visit (Doctor or Admin)
    /// </summary>
    [HttpPost]
    [AdminOrDoctor]
    public async Task<ActionResult<Result<PatientVisitDto>>> Create(
        [FromBody] CreatePatientVisitDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<PatientVisitDto>.Failure("Validation failed", errors));
        }

        var result = await _patientVisitService.CreateAsync(dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update an existing patient visit (Doctor or Admin)
    /// </summary>
    [HttpPut("{id}")]
    [AdminOrDoctor]
    public async Task<ActionResult<Result<PatientVisitDto>>> Update(
        Guid id,
        [FromBody] UpdatePatientVisitDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<PatientVisitDto>.Failure("Validation failed", errors));
        }

        var result = await _patientVisitService.UpdateAsync(id, dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a patient visit by ID (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [AdminOnly]
    public async Task<ActionResult<Result>> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _patientVisitService.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}

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
public class ImagingOrdersController : ControllerBase
{
    private readonly IImagingOrderService _imagingOrderService;
    private readonly IValidator<CreateImagingOrderDto> _createValidator;
    private readonly IValidator<UpdateImagingOrderDto> _updateValidator;

    public ImagingOrdersController(
        IImagingOrderService imagingOrderService,
        IValidator<CreateImagingOrderDto> createValidator,
        IValidator<UpdateImagingOrderDto> updateValidator)
    {
        _imagingOrderService = imagingOrderService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Get all imaging orders with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<ImagingOrderDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? visitId = null,
        [FromQuery] Guid? patientId = null,
        [FromQuery] Guid? requestingDoctorId = null,
        [FromQuery] string? modality = null,
        [FromQuery] string? bodyPart = null,
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

        var filters = new SearchImagingOrderDto
        {
            VisitId = visitId,
            PatientId = patientId,
            RequestingDoctorId = requestingDoctorId,
            Modality = modality,
            BodyPart = bodyPart,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _imagingOrderService.GetAllAsync(paginationParams, filters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get imaging order by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Result<ImagingOrderDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _imagingOrderService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new imaging order (Doctor or Admin)
    /// </summary>
    [HttpPost]
    [AdminOrDoctor]
    public async Task<ActionResult<Result<ImagingOrderDto>>> Create(
        [FromBody] CreateImagingOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<ImagingOrderDto>.Failure("Validation failed", errors));
        }

        var result = await _imagingOrderService.CreateAsync(dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Update an existing imaging order (Doctor or Admin)
    /// </summary>
    [HttpPut("{id}")]
    [AdminOrDoctor]
    public async Task<ActionResult<Result<ImagingOrderDto>>> Update(
        Guid id,
        [FromBody] UpdateImagingOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<ImagingOrderDto>.Failure("Validation failed", errors));
        }

        var result = await _imagingOrderService.UpdateAsync(id, dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete an imaging order by ID (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [AdminOnly]
    public async Task<ActionResult<Result>> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _imagingOrderService.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}

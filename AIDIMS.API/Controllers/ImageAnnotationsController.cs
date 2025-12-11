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
public class ImageAnnotationsController : ControllerBase
{
    private readonly IImageAnnotationService _annotationService;
    private readonly IValidator<CreateImageAnnotationDto> _createValidator;
    private readonly IValidator<UpdateImageAnnotationDto> _updateValidator;

    public ImageAnnotationsController(
        IImageAnnotationService annotationService,
        IValidator<CreateImageAnnotationDto> createValidator,
        IValidator<UpdateImageAnnotationDto> updateValidator)
    {
        _annotationService = annotationService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<ImageAnnotationDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? instanceId = null,
        [FromQuery] string? annotationType = null,
        CancellationToken cancellationToken = default)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var filters = new SearchImageAnnotationDto
        {
            InstanceId = instanceId,
            AnnotationType = annotationType
        };

        var result = await _annotationService.GetAllAsync(paginationParams, filters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<ImageAnnotationDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _annotationService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("instance/{instanceId}")]
    public async Task<ActionResult<Result<IEnumerable<ImageAnnotationDto>>>> GetByInstanceId(
        string instanceId,
        CancellationToken cancellationToken = default)
    {
        var result = await _annotationService.GetByInstanceIdAsync(instanceId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [AdminOrDoctor]
    public async Task<ActionResult<Result<ImageAnnotationDto>>> Create(
        [FromBody] CreateImageAnnotationDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<ImageAnnotationDto>.Failure("Validation failed", errors));
        }

        var result = await _annotationService.CreateAsync(dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("{id}")]
    [AdminOrDoctor]
    public async Task<ActionResult<Result<ImageAnnotationDto>>> Update(
        Guid id,
        [FromBody] UpdateImageAnnotationDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<ImageAnnotationDto>.Failure("Validation failed", errors));
        }

        var result = await _annotationService.UpdateAsync(id, dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [AdminOrDoctor]
    public async Task<ActionResult<Result>> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _annotationService.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}


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
public class DiagnosesController : ControllerBase
{
    private readonly IDiagnosisService _diagnosisService;
    private readonly IValidator<CreateDiagnosisDto> _createValidator;
    private readonly IValidator<UpdateDiagnosisDto> _updateValidator;

    public DiagnosesController(
        IDiagnosisService diagnosisService,
        IValidator<CreateDiagnosisDto> createValidator,
        IValidator<UpdateDiagnosisDto> updateValidator)
    {
        _diagnosisService = diagnosisService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<DiagnosisDto>>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? studyId = null,
        [FromQuery] string? reportStatus = null,
        [FromQuery] Guid? patientId = null,
        [FromQuery] Guid? doctorId = null,
        CancellationToken cancellationToken = default)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var filters = new SearchDiagnosisDto
        {
            StudyId = studyId,
            ReportStatus = reportStatus,
            PatientId = patientId,
            DoctorId = doctorId
        };

        var result = await _diagnosisService.GetAllAsync(paginationParams, filters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<DiagnosisDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _diagnosisService.GetByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("study/{studyId}")]
    public async Task<ActionResult<Result<DiagnosisDto>>> GetByStudyId(
        Guid studyId,
        CancellationToken cancellationToken = default)
    {
        var result = await _diagnosisService.GetByStudyIdAsync(studyId, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPost]
    [AdminOrDoctor]
    public async Task<ActionResult<Result<DiagnosisDto>>> Create(
        [FromBody] CreateDiagnosisDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<DiagnosisDto>.Failure("Validation failed", errors));
        }

        var result = await _diagnosisService.CreateAsync(dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("{id}")]
    [AdminOrDoctor]
    public async Task<ActionResult<Result<DiagnosisDto>>> Update(
        Guid id,
        [FromBody] UpdateDiagnosisDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<DiagnosisDto>.Failure("Validation failed", errors));
        }

        var result = await _diagnosisService.UpdateAsync(id, dto, cancellationToken);
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
        var result = await _diagnosisService.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}


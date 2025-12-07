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
public class DicomController : ControllerBase
{
    private readonly IDicomService _dicomService;
    private readonly IValidator<IFormFile> _fileValidator;

    public DicomController(
        IDicomService dicomService,
        IValidator<IFormFile> fileValidator)
    {
        _dicomService = dicomService;
        _fileValidator = fileValidator;
    }

    [HttpPost("upload")]
    public async Task<ActionResult<Result<DicomUploadResultDto>>> UploadDicomInstance(
        IFormFile file,
        [FromForm] Guid? orderId,
        [FromForm] Guid? patientId,
        CancellationToken cancellationToken)
    {
        if (!orderId.HasValue)
        {
            return BadRequest(Result<DicomUploadResultDto>.Failure("OrderId is required"));
        }

        if (!patientId.HasValue)
        {
            return BadRequest(Result<DicomUploadResultDto>.Failure("PatientId is required"));
        }

        var validationResult = await _fileValidator.ValidateAsync(file, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<DicomUploadResultDto>.Failure("Validation failed", errors));
        }

        var dicomDto = new DicomUploadDto
        {
            File = file,
            OrderId = orderId,
            PatientId = patientId
        };

        var result = await _dicomService.UploadInstanceAsync(dicomDto, cancellationToken);
        if (result == null)
        {
            return BadRequest(Result<DicomUploadResultDto>.Failure("Failed to upload DICOM instance"));
        }

        return Ok(Result<DicomUploadResultDto>.Success(result, "DICOM instance uploaded successfully"));
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<Result<IEnumerable<DicomInstanceDto>>>> GetDicomInstancesByOrderId(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var result = await _dicomService.GetInstancesByOrderIdAsync(orderId, cancellationToken);
        return Ok(Result<IEnumerable<DicomInstanceDto>>.Success(result, "DICOM instances retrieved successfully"));
    }

    [HttpGet("download/{instanceId}")]
    public async Task<IActionResult> DownloadDicomInstance(
        string instanceId,
        CancellationToken cancellationToken)
    {
        var result = await _dicomService.DownloadInstanceAsync(instanceId, cancellationToken);
        if (result == null)
        {
            return NotFound();
        }

        return File(result, "application/dicom", $"{instanceId}.dcm");
    }

    [HttpGet("preview/{instanceId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDicomPreview(
        string instanceId,
        CancellationToken cancellationToken)
    {
        var result = await _dicomService.GetInstancePreviewAsync(instanceId, cancellationToken);
        if (result == null)
        {
            return NotFound();
        }

        return File(result, "image/png");
    }
}

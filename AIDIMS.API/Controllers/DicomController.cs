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

    /// <summary>
    /// Upload a DICOM instance
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<Result<DicomUploadResultDto>>> UploadDicomInstance(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var validationResult = await _fileValidator.ValidateAsync(file, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result<DicomUploadResultDto>.Failure("Validation failed", errors));
        }

        var dicomDto = new DicomUploadDto
        {
            File = file
        };

        var result = await _dicomService.UploadInstanceAsync(dicomDto);
        if (result == null)
        {
            return BadRequest(Result<DicomUploadResultDto>.Failure("Failed to upload DICOM instance"));
        }

        return Ok(Result<DicomUploadResultDto>.Success(result, "DICOM instance uploaded successfully"));
    }
}

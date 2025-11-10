using AIDIMS.Application.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace AIDIMS.Application.Validators;

/// <summary>
/// Validator for IFormFile
/// </summary>
public class FormFileValidator : AbstractValidator<IFormFile>
{
    private const long MaxFileSize = 100 * 1024 * 1024;

    public FormFileValidator()
    {
        RuleFor(file => file.Length)
            .GreaterThan(0)
            .WithMessage("File cannot be empty.")
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage($"File size must be less than {MaxFileSize / 1024 / 1024}MB.");

        RuleFor(file => file.ContentType)
            .Must(contentType => contentType.Equals("application/dicom", StringComparison.OrdinalIgnoreCase) ||
                                 contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Invalid Content-Type.");
    }
}

/// <summary>
/// Validator for DicomUploadDto
/// </summary>
public class UploadDicomDtoValidator : AbstractValidator<DicomUploadDto>
{
    public UploadDicomDtoValidator()
    {
        RuleFor(dto => dto.File)
            .NotNull().WithMessage("File is required.")
            .SetValidator(new FormFileValidator());
    }
}

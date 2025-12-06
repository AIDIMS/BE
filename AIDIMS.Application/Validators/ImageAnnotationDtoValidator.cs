using AIDIMS.Application.DTOs;
using FluentValidation;

namespace AIDIMS.Application.Validators;

public class CreateImageAnnotationDtoValidator : AbstractValidator<CreateImageAnnotationDto>
{
    public CreateImageAnnotationDtoValidator()
    {
        RuleFor(x => x.InstanceId)
            .NotEmpty().WithMessage("Instance ID is required");

        RuleFor(x => x.AnnotationType)
            .NotEmpty().WithMessage("Annotation type is required")
            .MaximumLength(50).WithMessage("Annotation type must not exceed 50 characters");

        RuleFor(x => x.AnnotationData)
            .NotEmpty().WithMessage("Annotation data is required");
    }
}

public class UpdateImageAnnotationDtoValidator : AbstractValidator<UpdateImageAnnotationDto>
{
    public UpdateImageAnnotationDtoValidator()
    {
        RuleFor(x => x.AnnotationType)
            .NotEmpty().WithMessage("Annotation type is required")
            .MaximumLength(50).WithMessage("Annotation type must not exceed 50 characters");

        RuleFor(x => x.AnnotationData)
            .NotEmpty().WithMessage("Annotation data is required");
    }
}


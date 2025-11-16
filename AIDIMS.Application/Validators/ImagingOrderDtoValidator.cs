using AIDIMS.Application.DTOs;
using AIDIMS.Domain.Enums;
using FluentValidation;

namespace AIDIMS.Application.Validators;

public class CreateImagingOrderDtoValidator : AbstractValidator<CreateImagingOrderDto>
{
    public CreateImagingOrderDtoValidator()
    {
        RuleFor(x => x.VisitId)
            .NotEmpty()
            .WithMessage("Visit ID is required");

        RuleFor(x => x.RequestingDoctorId)
            .NotEmpty()
            .WithMessage("Requesting doctor ID is required");

        RuleFor(x => x.ModalityRequested)
            .NotEmpty()
            .WithMessage("Modality is required")
            .Must(BeValidModality)
            .WithMessage("Invalid modality. Valid values: XRay, CTScan, MRI, Ultrasound, Mammography, Fluoroscopy, NuclearMedicine");

        RuleFor(x => x.BodyPartRequested)
            .NotEmpty()
            .WithMessage("Body part is required")
            .Must(BeValidBodyPart)
            .WithMessage("Invalid body part. Valid values: Head, Chest, Brain, Abdomen, Pelvis, Extremities, Spine, Neck, Other");

        RuleFor(x => x.ReasonForStudy)
            .MaximumLength(1000)
            .WithMessage("Reason for study cannot exceed 1000 characters");
    }

    private bool BeValidModality(string modality)
    {
        return Enum.TryParse<Modality>(modality, true, out _);
    }

    private bool BeValidBodyPart(string bodyPart)
    {
        return Enum.TryParse<BodyPart>(bodyPart, true, out _);
    }
}

public class UpdateImagingOrderDtoValidator : AbstractValidator<UpdateImagingOrderDto>
{
    public UpdateImagingOrderDtoValidator()
    {
        When(x => !string.IsNullOrWhiteSpace(x.ModalityRequested), () =>
        {
            RuleFor(x => x.ModalityRequested)
                .Must(BeValidModality!)
                .WithMessage("Invalid modality. Valid values: XRay, CTScan, MRI, Ultrasound, Mammography, Fluoroscopy, NuclearMedicine");
        });

        When(x => !string.IsNullOrWhiteSpace(x.BodyPartRequested), () =>
        {
            RuleFor(x => x.BodyPartRequested)
                .Must(BeValidBodyPart!)
                .WithMessage("Invalid body part. Valid values: Head, Chest, Brain, Abdomen, Pelvis, Extremities, Spine, Neck, Other");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Status), () =>
        {
            RuleFor(x => x.Status)
                .Must(BeValidStatus!)
                .WithMessage("Invalid status. Valid values: Pending, Completed, Cancelled");
        });

        RuleFor(x => x.ReasonForStudy)
            .MaximumLength(1000)
            .WithMessage("Reason for study cannot exceed 1000 characters");
    }

    private bool BeValidModality(string modality)
    {
        return Enum.TryParse<Modality>(modality, true, out _);
    }

    private bool BeValidBodyPart(string bodyPart)
    {
        return Enum.TryParse<BodyPart>(bodyPart, true, out _);
    }

    private bool BeValidStatus(string status)
    {
        return Enum.TryParse<ImagingOrderStatus>(status, true, out _);
    }
}

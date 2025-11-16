using AIDIMS.Application.DTOs;
using AIDIMS.Domain.Enums;
using FluentValidation;

namespace AIDIMS.Application.Validators;

public class CreatePatientVisitDtoValidator : AbstractValidator<CreatePatientVisitDto>
{
    public CreatePatientVisitDtoValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("Patient ID is required");

        RuleFor(x => x.AssignedDoctorId)
            .NotEmpty()
            .WithMessage("Assigned doctor ID is required");

        RuleFor(x => x.Symptoms)
            .MaximumLength(1000)
            .WithMessage("Symptoms cannot exceed 1000 characters");
    }
}

public class UpdatePatientVisitDtoValidator : AbstractValidator<UpdatePatientVisitDto>
{
    public UpdatePatientVisitDtoValidator()
    {
        When(x => x.AssignedDoctorId.HasValue, () =>
        {
            RuleFor(x => x.AssignedDoctorId!.Value)
                .NotEmpty()
                .WithMessage("Assigned doctor ID cannot be empty");
        });

        RuleFor(x => x.Symptoms)
            .MaximumLength(1000)
            .WithMessage("Symptoms cannot exceed 1000 characters");

        When(x => !string.IsNullOrWhiteSpace(x.Status), () =>
        {
            RuleFor(x => x.Status)
                .Must(BeValidStatus!)
                .WithMessage("Invalid status. Valid values: Waiting, Inprogress, Done");
        });
    }

    private bool BeValidStatus(string status)
    {
        return Enum.TryParse<PatientVisitStatus>(status, true, out _);
    }
}

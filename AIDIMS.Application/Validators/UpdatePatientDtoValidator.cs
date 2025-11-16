using AIDIMS.Application.DTOs;
using AIDIMS.Domain.Enums;
using FluentValidation;

namespace AIDIMS.Application.Validators;

public class UpdatePatientDtoValidator : AbstractValidator<UpdatePatientDto>
{
    public UpdatePatientDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.Now).WithMessage("Date of birth must be in the past");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required")
            .Must(BeValidGender)
            .WithMessage("Invalid gender. Valid values: Male, Female, Other");
        
        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(?:\+84|0)\d{9}$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }

    private bool BeValidGender(string gender)
    {
        return Enum.TryParse<Gender>(gender, true, out _);
    }
}

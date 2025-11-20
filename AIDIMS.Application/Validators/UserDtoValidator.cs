using AIDIMS.Application.DTOs;
using FluentValidation;

namespace AIDIMS.Application.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(255).WithMessage("Password must not exceed 255 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(?:\+84|0)\d{9}$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .MaximumLength(15).WithMessage("Phone number must not exceed 15 characters");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("User role is required")
            .IsInEnum().WithMessage("Invalid user role");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required")
            .IsInEnum().WithMessage("Invalid department");
    }
}

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(?:\+84|0)\d{9}$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .MaximumLength(15).WithMessage("Phone number must not exceed 15 characters");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("User role is required")
            .IsInEnum().WithMessage("Invalid user role");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required")
            .IsInEnum().WithMessage("Invalid department");
    }
}

public class UpdateUserByIdentifyDtoValidator : AbstractValidator<UpdateUserByIdentifyDto>
{
    public UpdateUserByIdentifyDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(?:\+84|0)\d{9}$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .MaximumLength(15).WithMessage("Phone number must not exceed 15 characters");
    }
}

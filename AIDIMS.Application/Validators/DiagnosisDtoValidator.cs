using AIDIMS.Application.DTOs;
using AIDIMS.Domain.Enums;
using FluentValidation;

namespace AIDIMS.Application.Validators;

public class CreateDiagnosisDtoValidator : AbstractValidator<CreateDiagnosisDto>
{
    public CreateDiagnosisDtoValidator()
    {
        RuleFor(x => x.StudyId)
            .NotEmpty().WithMessage("Study ID is required");

        RuleFor(x => x.FinalDiagnosis)
            .NotEmpty().WithMessage("Final diagnosis is required")
            .MaximumLength(5000).WithMessage("Final diagnosis must not exceed 5000 characters");

        RuleFor(x => x.ReportStatus)
            .NotEmpty().WithMessage("Report status is required")
            .Must(BeValidStatus).WithMessage("Invalid report status. Valid values are: Draft, Completed, Approved");
    }

    private bool BeValidStatus(string status)
    {
        return Enum.TryParse<DiagnosisReportStatus>(status, true, out _);
    }
}

public class UpdateDiagnosisDtoValidator : AbstractValidator<UpdateDiagnosisDto>
{
    public UpdateDiagnosisDtoValidator()
    {
        RuleFor(x => x.FinalDiagnosis)
            .NotEmpty().WithMessage("Final diagnosis is required")
            .MaximumLength(5000).WithMessage("Final diagnosis must not exceed 5000 characters");

        RuleFor(x => x.ReportStatus)
            .NotEmpty().WithMessage("Report status is required")
            .Must(BeValidStatus).WithMessage("Invalid report status. Valid values are: Draft, Completed, Approved");
    }

    private bool BeValidStatus(string status)
    {
        return Enum.TryParse<DiagnosisReportStatus>(status, true, out _);
    }
}


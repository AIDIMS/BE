using System.Security.Claims;
using AIDIMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace AIDIMS.API.Filters;

/// <summary>
/// Handler for Admin authorization requirement
/// </summary>
public class AdminRequirementHandler : AuthorizationHandler<AdminRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);

        if (roleClaim != null && roleClaim.Value == UserRole.Admin.ToString())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler for Doctor authorization requirement
/// </summary>
public class DoctorRequirementHandler : AuthorizationHandler<DoctorRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DoctorRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);

        if (roleClaim != null && roleClaim.Value == UserRole.Doctor.ToString())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler for Technician authorization requirement
/// </summary>
public class TechnicianRequirementHandler : AuthorizationHandler<TechnicianRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TechnicianRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);

        if (roleClaim != null && roleClaim.Value == UserRole.Technician.ToString())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler for Receptionist authorization requirement
/// </summary>
public class ReceptionistRequirementHandler : AuthorizationHandler<ReceptionistRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ReceptionistRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);

        if (roleClaim != null && roleClaim.Value == UserRole.Receptionist.ToString())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handler for Medical Staff authorization requirement (Doctor or Technician)
/// </summary>
public class MedicalStaffRequirementHandler : AuthorizationHandler<MedicalStaffRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MedicalStaffRequirement requirement)
    {
        var roleClaim = context.User.FindFirst(ClaimTypes.Role);

        if (roleClaim != null &&
            (roleClaim.Value == UserRole.Doctor.ToString() ||
             roleClaim.Value == UserRole.Technician.ToString()))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

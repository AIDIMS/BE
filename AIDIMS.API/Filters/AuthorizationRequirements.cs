using Microsoft.AspNetCore.Authorization;

namespace AIDIMS.API.Filters;

/// <summary>
/// Authorization requirement for Admin role
/// </summary>
public class AdminRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for Doctor role
/// </summary>
public class DoctorRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for Technician role
/// </summary>
public class TechnicianRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for Receptionist role
/// </summary>
public class ReceptionistRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Authorization requirement for Medical Staff (Doctor or Technician)
/// </summary>
public class MedicalStaffRequirement : IAuthorizationRequirement
{
}

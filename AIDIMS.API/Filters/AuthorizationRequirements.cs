using Microsoft.AspNetCore.Authorization;

namespace AIDIMS.API.Filters;

public class AdminRequirement : IAuthorizationRequirement
{
}

public class DoctorRequirement : IAuthorizationRequirement
{
}

public class TechnicianRequirement : IAuthorizationRequirement
{
}

public class ReceptionistRequirement : IAuthorizationRequirement
{
}

public class MedicalStaffRequirement : IAuthorizationRequirement
{
}

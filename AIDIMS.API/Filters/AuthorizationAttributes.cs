using Microsoft.AspNetCore.Authorization;

namespace AIDIMS.API.Filters;

/// <summary>
/// Attribute to require Admin role
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class AdminOnlyAttribute : AuthorizeAttribute
{
    public AdminOnlyAttribute()
    {
        Policy = PolicyNames.AdminOnly;
    }
}

/// <summary>
/// Attribute to require Doctor role
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class DoctorOnlyAttribute : AuthorizeAttribute
{
    public DoctorOnlyAttribute()
    {
        Policy = PolicyNames.DoctorOnly;
    }
}

/// <summary>
/// Attribute to require Technician role
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class TechnicianOnlyAttribute : AuthorizeAttribute
{
    public TechnicianOnlyAttribute()
    {
        Policy = PolicyNames.TechnicianOnly;
    }
}

/// <summary>
/// Attribute to require Receptionist role
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ReceptionistOnlyAttribute : AuthorizeAttribute
{
    public ReceptionistOnlyAttribute()
    {
        Policy = PolicyNames.ReceptionistOnly;
    }
}

/// <summary>
/// Attribute to require Medical Staff role (Doctor or Technician)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class MedicalStaffOnlyAttribute : AuthorizeAttribute
{
    public MedicalStaffOnlyAttribute()
    {
        Policy = PolicyNames.MedicalStaffOnly;
    }
}

/// <summary>
/// Attribute to require Admin or Doctor role
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class AdminOrDoctorAttribute : AuthorizeAttribute
{
    public AdminOrDoctorAttribute()
    {
        Policy = PolicyNames.AdminOrDoctor;
    }
}

/// <summary>
/// Attribute to require Admin or Technician role
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class AdminOrTechnicianAttribute : AuthorizeAttribute
{
    public AdminOrTechnicianAttribute()
    {
        Policy = PolicyNames.AdminOrTechnician;
    }
}

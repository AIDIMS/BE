using Microsoft.AspNetCore.Authorization;

namespace AIDIMS.API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class AdminOnlyAttribute : AuthorizeAttribute
{
    public AdminOnlyAttribute()
    {
        Policy = PolicyNames.AdminOnly;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class DoctorOnlyAttribute : AuthorizeAttribute
{
    public DoctorOnlyAttribute()
    {
        Policy = PolicyNames.DoctorOnly;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class TechnicianOnlyAttribute : AuthorizeAttribute
{
    public TechnicianOnlyAttribute()
    {
        Policy = PolicyNames.TechnicianOnly;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class ReceptionistOnlyAttribute : AuthorizeAttribute
{
    public ReceptionistOnlyAttribute()
    {
        Policy = PolicyNames.ReceptionistOnly;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class MedicalStaffOnlyAttribute : AuthorizeAttribute
{
    public MedicalStaffOnlyAttribute()
    {
        Policy = PolicyNames.MedicalStaffOnly;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class AdminOrDoctorAttribute : AuthorizeAttribute
{
    public AdminOrDoctorAttribute()
    {
        Policy = PolicyNames.AdminOrDoctor;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class AdminOrTechnicianAttribute : AuthorizeAttribute
{
    public AdminOrTechnicianAttribute()
    {
        Policy = PolicyNames.AdminOrTechnician;
    }
}


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class AdminOrReceptionistAttribute : AuthorizeAttribute
{
    public AdminOrReceptionistAttribute()
    {
        Policy = PolicyNames.AdminOrReceptionist;
    }
}

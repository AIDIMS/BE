namespace AIDIMS.Application.DTOs;

/// <summary>
/// Dashboard overview statistics
/// </summary>
public class DashboardStatisticsDto
{
    public int TotalPatients { get; set; }
    public double TotalPatientsChange { get; set; }
    public int VisitsToday { get; set; }
    public int VisitsTodayChange { get; set; }
    public int ImagingOrdersPending { get; set; }
    public int ImagingOrdersTotal { get; set; }
    public int AverageWaitTimeMinutes { get; set; }
    public double AverageWaitTimeChange { get; set; }
}

/// <summary>
/// Department staff distribution statistics
/// </summary>
public class DepartmentStatDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int StaffCount { get; set; }
    public double Percentage { get; set; }
}

public class DepartmentStatisticsDto
{
    public List<DepartmentStatDto> Departments { get; set; } = new();
    public int TotalStaff { get; set; }
    public int TotalActiveStaff { get; set; }
}

/// <summary>
/// Weekly activity data
/// </summary>
public class WeeklyActivityItemDto
{
    public string Date { get; set; } = string.Empty;
    public string DayOfWeek { get; set; } = string.Empty;
    public string DayOfWeekVi { get; set; } = string.Empty;
    public int VisitCount { get; set; }
    public int ImagingOrderCount { get; set; }
}

public class WeeklyActivityDto
{
    public List<WeeklyActivityItemDto> Activities { get; set; } = new();
    public string WeekStartDate { get; set; } = string.Empty;
    public string WeekEndDate { get; set; } = string.Empty;
}

/// <summary>
/// Combined dashboard data
/// </summary>
public class DashboardDataDto
{
    public DashboardStatisticsDto? Statistics { get; set; }
    public DepartmentStatisticsDto? DepartmentStatistics { get; set; }
    public WeeklyActivityDto? WeeklyActivity { get; set; }
}


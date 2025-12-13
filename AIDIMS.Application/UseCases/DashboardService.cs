using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using AIDIMS.Domain.Entities;
using AIDIMS.Domain.Enums;
using AIDIMS.Domain.Interfaces;

namespace AIDIMS.Application.UseCases;

public class DashboardService : IDashboardService
{
    private readonly IRepository<Patient> _patientRepository;
    private readonly IRepository<PatientVisit> _visitRepository;
    private readonly IRepository<ImagingOrder> _orderRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DashboardService(
        IRepository<Patient> patientRepository,
        IRepository<PatientVisit> visitRepository,
        IRepository<ImagingOrder> orderRepository,
        IRepository<User> userRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _patientRepository = patientRepository;
        _visitRepository = visitRepository;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<DashboardStatisticsDto>> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current Vietnam time and convert to Unspecified for comparison with database dates
            var today = DateTime.SpecifyKind(_dateTimeProvider.Today, DateTimeKind.Unspecified);
            var yesterday = today.AddDays(-1);
            var lastMonth = today.AddMonths(-1);

            // Get all patients
            var allPatients = await _patientRepository.GetAllAsync(cancellationToken);
            var totalPatients = allPatients.Count();

            // Calculate patients last month (patients created before last month)
            var totalPatientsLastMonth = allPatients
                .Count(p => p.CreatedAt.Date < lastMonth);

            // Get all visits
            var allVisits = await _visitRepository.GetAllAsync(cancellationToken);

            // Calculate visits today and yesterday
            var visitsToday = allVisits
                .Count(v => v.CreatedAt.Date == today);

            var visitsYesterday = allVisits
                .Count(v => v.CreatedAt.Date == yesterday);

            // Get all imaging orders
            var allOrders = await _orderRepository.GetAllAsync(cancellationToken);

            // Calculate pending and total imaging orders
            var imagingOrdersPending = allOrders
                .Count(o => o.Status == ImagingOrderStatus.Pending);

            var imagingOrdersTotal = allOrders
                .Count(o => o.CreatedAt.Date >= today.AddDays(-30)); // Last 30 days

            // Calculate average wait time (time between visit creation and first order creation)
            var recentVisits = allVisits
                .Where(v => v.CreatedAt.Date >= today.AddDays(-7)) // Last week
                .ToList();

            var waitTimes = new List<int>();
            foreach (var visit in recentVisits)
            {
                var firstOrder = allOrders
                    .Where(o => o.VisitId == visit.Id)
                    .OrderBy(o => o.CreatedAt)
                    .FirstOrDefault();

                if (firstOrder != null)
                {
                    var waitTime = (int)(firstOrder.CreatedAt - visit.CreatedAt).TotalMinutes;
                    if (waitTime >= 0 && waitTime < 1440) // Filter out invalid data (> 24 hours)
                    {
                        waitTimes.Add(waitTime);
                    }
                }
            }

            var averageWaitTimeMinutes = waitTimes.Any() ? (int)waitTimes.Average() : 0;

            // Calculate average wait time for previous week
            var previousWeekVisits = allVisits
                .Where(v => v.CreatedAt.Date >= today.AddDays(-14) && v.CreatedAt.Date < today.AddDays(-7))
                .ToList();

            var previousWaitTimes = new List<int>();
            foreach (var visit in previousWeekVisits)
            {
                var firstOrder = allOrders
                    .Where(o => o.VisitId == visit.Id)
                    .OrderBy(o => o.CreatedAt)
                    .FirstOrDefault();

                if (firstOrder != null)
                {
                    var waitTime = (int)(firstOrder.CreatedAt - visit.CreatedAt).TotalMinutes;
                    if (waitTime >= 0 && waitTime < 1440)
                    {
                        previousWaitTimes.Add(waitTime);
                    }
                }
            }

            var previousAverageWaitTime = previousWaitTimes.Any() ? (int)previousWaitTimes.Average() : averageWaitTimeMinutes;

            // Calculate percentage change for patients
            var totalPatientsChange = CalculatePercentageChange(totalPatients, totalPatientsLastMonth);

            // Calculate change for wait time
            var averageWaitTimeChange = CalculatePercentageChange(averageWaitTimeMinutes, previousAverageWaitTime);

            var statistics = new DashboardStatisticsDto
            {
                TotalPatients = totalPatients,
                TotalPatientsChange = totalPatientsChange,
                VisitsToday = visitsToday,
                VisitsTodayChange = visitsToday - visitsYesterday,
                ImagingOrdersPending = imagingOrdersPending,
                ImagingOrdersTotal = imagingOrdersTotal,
                AverageWaitTimeMinutes = averageWaitTimeMinutes,
                AverageWaitTimeChange = averageWaitTimeChange
            };

            return Result<DashboardStatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            return Result<DashboardStatisticsDto>.Failure($"Failed to get dashboard statistics: {ex.Message}");
        }
    }

    public async Task<Result<DepartmentStatisticsDto>> GetDepartmentStatisticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all users (staff) from database
            var allUsers = await _userRepository.GetAllAsync(cancellationToken);
            
            // Filter out deleted users and get active staff
            var activeStaff = allUsers.Where(u => !u.IsDeleted).ToList();
            var totalStaff = activeStaff.Count;

            // Group by department
            var departmentGroups = activeStaff
                .GroupBy(u => u.Department)
                .Select(g => new DepartmentStatDto
                {
                    DepartmentName = GetDepartmentDisplayName(g.Key),
                    StaffCount = g.Count(),
                    Percentage = totalStaff > 0 ? Math.Round((double)g.Count() / totalStaff * 100, 1) : 0
                })
                .OrderByDescending(d => d.StaffCount)
                .ToList();

            var statistics = new DepartmentStatisticsDto
            {
                Departments = departmentGroups,
                TotalStaff = totalStaff,
                TotalActiveStaff = totalStaff
            };

            return Result<DepartmentStatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            return Result<DepartmentStatisticsDto>.Failure($"Failed to get department statistics: {ex.Message}");
        }
    }

    public async Task<Result<WeeklyActivityDto>> GetWeeklyActivityAsync(
        int weekOffset = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current Vietnam time and convert to Unspecified for comparison with database dates
            var today = DateTime.SpecifyKind(_dateTimeProvider.Today, DateTimeKind.Unspecified);
            var targetWeekStart = GetStartOfWeek(today).AddDays(weekOffset * 7);
            var targetWeekEnd = targetWeekStart.AddDays(6);

            // Get all visits and orders
            var allVisits = await _visitRepository.GetAllAsync(cancellationToken);
            var allOrders = await _orderRepository.GetAllAsync(cancellationToken);

            var activities = new List<WeeklyActivityItemDto>();

            // Generate data for each day of the week
            for (int i = 0; i < 7; i++)
            {
                var date = targetWeekStart.AddDays(i);
                var visitCount = allVisits.Count(v => v.CreatedAt.Date == date);
                var orderCount = allOrders.Count(o => o.CreatedAt.Date == date);

                activities.Add(new WeeklyActivityItemDto
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    DayOfWeek = date.DayOfWeek.ToString(),
                    DayOfWeekVi = GetVietnameseDayOfWeek(date.DayOfWeek),
                    VisitCount = visitCount,
                    ImagingOrderCount = orderCount
                });
            }

            var weeklyActivity = new WeeklyActivityDto
            {
                Activities = activities,
                WeekStartDate = targetWeekStart.ToString("yyyy-MM-dd"),
                WeekEndDate = targetWeekEnd.ToString("yyyy-MM-dd")
            };

            return Result<WeeklyActivityDto>.Success(weeklyActivity);
        }
        catch (Exception ex)
        {
            return Result<WeeklyActivityDto>.Failure($"Failed to get weekly activity: {ex.Message}");
        }
    }

    public async Task<Result<DashboardDataDto>> GetDashboardDataAsync(
        bool includeStatistics = true,
        bool includeDepartments = true,
        bool includeWeeklyActivity = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dashboardData = new DashboardDataDto();

            if (includeStatistics)
            {
                var statsResult = await GetStatisticsAsync(cancellationToken);
                if (statsResult.IsSuccess)
                {
                    dashboardData.Statistics = statsResult.Data;
                }
            }

            if (includeDepartments)
            {
                var deptResult = await GetDepartmentStatisticsAsync(null, null, cancellationToken);
                if (deptResult.IsSuccess)
                {
                    dashboardData.DepartmentStatistics = deptResult.Data;
                }
            }

            if (includeWeeklyActivity)
            {
                var activityResult = await GetWeeklyActivityAsync(0, cancellationToken);
                if (activityResult.IsSuccess)
                {
                    dashboardData.WeeklyActivity = activityResult.Data;
                }
            }

            return Result<DashboardDataDto>.Success(dashboardData);
        }
        catch (Exception ex)
        {
            return Result<DashboardDataDto>.Failure($"Failed to get dashboard data: {ex.Message}");
        }
    }

    #region Helper Methods

    private double CalculatePercentageChange(int current, int previous)
    {
        if (previous == 0)
        {
            return current > 0 ? 100.0 : 0.0;
        }
        return Math.Round(((double)(current - previous) / previous) * 100, 1);
    }

    private DateTime GetStartOfWeek(DateTime date)
    {
        // In Vietnam, Monday is the start of the week
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private string GetVietnameseDayOfWeek(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "Thứ Hai",
            DayOfWeek.Tuesday => "Thứ Ba",
            DayOfWeek.Wednesday => "Thứ Tư",
            DayOfWeek.Thursday => "Thứ Năm",
            DayOfWeek.Friday => "Thứ Sáu",
            DayOfWeek.Saturday => "Thứ Bảy",
            DayOfWeek.Sunday => "Chủ Nhật",
            _ => dayOfWeek.ToString()
        };
    }

    private string GetDepartmentDisplayName(Department department)
    {
        return department switch
        {
            Department.Administration => "Hành chính",
            Department.Pulmonology => "Nội khoa",
            Department.Radiology => "Chẩn đoán hình ảnh",
            Department.LungFunction => "Chức năng hô hấp",
            _ => department.ToString()
        };
    }

    #endregion
}


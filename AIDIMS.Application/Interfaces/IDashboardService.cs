using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;

namespace AIDIMS.Application.Interfaces;

public interface IDashboardService
{
    /// <summary>
    /// Get dashboard overview statistics
    /// </summary>
    Task<Result<DashboardStatisticsDto>> GetStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get department staff distribution statistics
    /// </summary>
    Task<Result<DepartmentStatisticsDto>> GetDepartmentStatisticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get weekly activity data
    /// </summary>
    Task<Result<WeeklyActivityDto>> GetWeeklyActivityAsync(
        int weekOffset = 0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all dashboard data in one call
    /// </summary>
    Task<Result<DashboardDataDto>> GetDashboardDataAsync(
        bool includeStatistics = true,
        bool includeDepartments = true,
        bool includeWeeklyActivity = true,
        CancellationToken cancellationToken = default);
}


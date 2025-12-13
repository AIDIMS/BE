using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIDIMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Get dashboard overview statistics
    /// </summary>
    /// <returns>Dashboard statistics including total patients, visits today, imaging orders, and wait times</returns>
    [HttpGet("statistics")]
    public async Task<ActionResult<Result<DashboardStatisticsDto>>> GetStatistics(
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetStatisticsAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get department statistics
    /// </summary>
    /// <param name="startDate">Start date for filtering (default: start of current week)</param>
    /// <param name="endDate">End date for filtering (default: today)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Department distribution with visit counts and percentages</returns>
    [HttpGet("department-statistics")]
    public async Task<ActionResult<Result<DepartmentStatisticsDto>>> GetDepartmentStatistics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetDepartmentStatisticsAsync(
            startDate,
            endDate,
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get weekly activity data
    /// </summary>
    /// <param name="weekOffset">Number of weeks to offset from current week (0 = current week, -1 = previous week)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Activity data for 7 days including visit and imaging order counts</returns>
    [HttpGet("weekly-activity")]
    public async Task<ActionResult<Result<WeeklyActivityDto>>> GetWeeklyActivity(
        [FromQuery] int weekOffset = 0,
        CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetWeeklyActivityAsync(weekOffset, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get all dashboard data in one call
    /// </summary>
    /// <param name="includeStatistics">Include overview statistics (default: true)</param>
    /// <param name="includeDepartments">Include department statistics (default: true)</param>
    /// <param name="includeWeeklyActivity">Include weekly activity (default: true)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Combined dashboard data</returns>
    [HttpGet]
    public async Task<ActionResult<Result<DashboardDataDto>>> GetDashboard(
        [FromQuery] bool includeStatistics = true,
        [FromQuery] bool includeDepartments = true,
        [FromQuery] bool includeWeeklyActivity = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetDashboardDataAsync(
            includeStatistics,
            includeDepartments,
            includeWeeklyActivity,
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Alternative endpoint for statistics (alias)
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<Result<DashboardStatisticsDto>>> GetStats(
        CancellationToken cancellationToken)
    {
        return await GetStatistics(cancellationToken);
    }

    /// <summary>
    /// Alternative endpoint for department statistics (alias)
    /// </summary>
    [HttpGet("departments/stats")]
    public async Task<ActionResult<Result<DepartmentStatisticsDto>>> GetDepartmentsStats(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        return await GetDepartmentStatistics(startDate, endDate, cancellationToken);
    }

    /// <summary>
    /// Alternative endpoint for weekly activity (alias)
    /// </summary>
    [HttpGet("activity/weekly")]
    public async Task<ActionResult<Result<WeeklyActivityDto>>> GetActivityWeekly(
        [FromQuery] int weekOffset = 0,
        CancellationToken cancellationToken = default)
    {
        return await GetWeeklyActivity(weekOffset, cancellationToken);
    }
}


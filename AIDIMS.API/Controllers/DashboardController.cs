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

    [HttpGet("statistics")]
    public async Task<ActionResult<Result<DashboardStatisticsDto>>> GetStatistics(
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetStatisticsAsync(cancellationToken);
        return Ok(result);
    }


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

    [HttpGet("weekly-activity")]
    public async Task<ActionResult<Result<WeeklyActivityDto>>> GetWeeklyActivity(
        [FromQuery] int weekOffset = 0,
        CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetWeeklyActivityAsync(weekOffset, cancellationToken);
        return Ok(result);
    }

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


    [HttpGet("stats")]
    public async Task<ActionResult<Result<DashboardStatisticsDto>>> GetStats(
        CancellationToken cancellationToken)
    {
        return await GetStatistics(cancellationToken);
    }


    [HttpGet("departments/stats")]
    public async Task<ActionResult<Result<DepartmentStatisticsDto>>> GetDepartmentsStats(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        return await GetDepartmentStatistics(startDate, endDate, cancellationToken);
    }


    [HttpGet("activity/weekly")]
    public async Task<ActionResult<Result<WeeklyActivityDto>>> GetActivityWeekly(
        [FromQuery] int weekOffset = 0,
        CancellationToken cancellationToken = default)
    {
        return await GetWeeklyActivity(weekOffset, cancellationToken);
    }
}


using AIDIMS.Application.Common;
using AIDIMS.Application.DTOs;
using AIDIMS.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIDIMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiAnalysisController : ControllerBase
{
    private readonly IAiAnalysisService _aiAnalysisService;

    public AiAnalysisController(IAiAnalysisService aiAnalysisService)
    {
        _aiAnalysisService = aiAnalysisService;
    }

    /// <summary>
    /// Gửi DICOM study đến AI service để phân tích
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<Result<AiAnalysisResponseDto>>> AnalyzeDicomStudy(
        [FromBody] AnalyzeDicomRequestDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiAnalysisService.AnalyzeDicomStudyAsync(dto.StudyId, dto.InstanceId, cancellationToken);
            return Ok(Result<AiAnalysisResponseDto>.Success(result, "DICOM study analyzed successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(Result<AiAnalysisResponseDto>.Failure($"Failed to analyze DICOM study: {ex.Message}"));
        }
    }

    /// <summary>
    /// Nhận kết quả từ AI service và lưu vào database (dùng khi AI service callback)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Result<AiAnalysisResponseDto>>> CreateAnalysis(
        [FromBody] CreateAiAnalysisDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _aiAnalysisService.CreateAnalysisAsync(dto, cancellationToken);
            return Ok(Result<AiAnalysisResponseDto>.Success(result, "AI analysis created successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(Result<AiAnalysisResponseDto>.Failure($"Failed to create AI analysis: {ex.Message}"));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<AiAnalysisResponseDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _aiAnalysisService.GetByIdAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound(Result<AiAnalysisResponseDto>.Failure("AI analysis not found"));
        }

        return Ok(Result<AiAnalysisResponseDto>.Success(result));
    }

    [HttpGet("study/{studyId}")]
    public async Task<ActionResult<Result<AiAnalysisResponseDto>>> GetByStudyId(
        Guid studyId,
        CancellationToken cancellationToken)
    {
        var result = await _aiAnalysisService.GetByStudyIdAsync(studyId, cancellationToken);
        if (result == null)
        {
            return NotFound(Result<AiAnalysisResponseDto>.Failure("AI analysis not found for this study"));
        }

        return Ok(Result<AiAnalysisResponseDto>.Success(result));
    }

    [HttpGet("instance/{instanceId}")]
    public async Task<ActionResult<Result<AiAnalysisResponseDto>>> GetByInstanceId(
        string instanceId,
        CancellationToken cancellationToken)
    {
        var result = await _aiAnalysisService.GetByOrthancInstanceIdAsync(instanceId, cancellationToken);
        if (result == null)
        {
            return NotFound(Result<AiAnalysisResponseDto>.Failure("AI analysis not found for this instance"));
        }

        return Ok(Result<AiAnalysisResponseDto>.Success(result));
    }

    [HttpPatch("{id}/review")]
    public async Task<ActionResult<Result<bool>>> MarkAsReviewed(
        Guid id,
        CancellationToken cancellationToken)
    {
        var success = await _aiAnalysisService.MarkAsReviewedAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(Result<bool>.Failure("AI analysis not found"));
        }

        return Ok(Result<bool>.Success(true, "AI analysis marked as reviewed"));
    }

    /// <summary>
    /// Kiểm tra xem study có thể sử dụng AI analysis không
    /// </summary>
    [HttpGet("availability/study/{studyId}")]
    public async Task<ActionResult<Result<AiAvailabilityDto>>> CheckAvailability(
        Guid studyId,
        CancellationToken cancellationToken)
    {
        try
        {
            var availability = await _aiAnalysisService.CheckAiAvailabilityAsync(studyId, cancellationToken);
            return Ok(Result<AiAvailabilityDto>.Success(availability));
        }
        catch (Exception ex)
        {
            return BadRequest(Result<AiAvailabilityDto>.Failure($"Failed to check AI availability: {ex.Message}"));
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReactionController : ControllerBase
{
    private readonly IReactionService _reactionService;

    public ReactionController(IReactionService reactionService)
    {
        _reactionService = reactionService;
    }

    [HttpGet("report/{reportId}/strategist/{strategistId}")]
    public async Task<ActionResult<ReactionDto>> GetByReportAndStrategist(Guid reportId, Guid strategistId)
    {
        var reaction = await _reactionService.GetByReportAndStrategistAsync(reportId, strategistId);
        if (reaction == null)
            return NotFound($"Reaction not found for report {reportId} and strategist {strategistId}.");

        return Ok(reaction);
    }

    [HttpGet("report/{reportId}")]
    public async Task<ActionResult<IEnumerable<ReactionDto>>> GetByReportId(Guid reportId)
    {
        try
        {
            var reactions = await _reactionService.GetByReportIdAsync(reportId);
            return Ok(reactions);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("report/{reportId}/summary")]
    public async Task<ActionResult<ReactionsSummaryDto>> GetReactionsSummary(Guid reportId)
    {
        try
        {
            var summary = await _reactionService.GetReactionsSummaryAsync(reportId);
            return Ok(summary);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("reports/{reportId}")]
    public async Task<ActionResult<ReactionDto>> AddOrUpdate(Guid reportId, [FromBody] CreateReactionDto createDto)
    {
        try
        {
            // Ensure reportId matches
            createDto.ReportId = reportId;
            
            var reaction = await _reactionService.AddOrUpdateAsync(createDto);
            return Ok(reaction);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("reports/{reportId}")]
    public async Task<ActionResult> Remove(Guid reportId, [FromQuery] Guid strategistId)
    {
        var deleted = await _reactionService.RemoveAsync(reportId, strategistId);
        if (!deleted)
            return NotFound($"Reaction not found for report {reportId} and strategist {strategistId}.");

        return NoContent();
    }
}


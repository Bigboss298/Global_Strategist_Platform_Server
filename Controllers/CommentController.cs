using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CommentDto>> GetById(Guid id)
    {
        var comment = await _commentService.GetByIdAsync(id);
        if (comment == null)
            return NotFound($"Comment with ID {id} not found.");

        return Ok(comment);
    }

    [HttpGet("report/{reportId}")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetByReportId(Guid reportId)
    {
        try
        {
            var comments = await _commentService.GetByReportIdAsync(reportId);
            return Ok(comments);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("reports/{reportId}")]
    public async Task<ActionResult<CommentDto>> Create(Guid reportId, [FromBody] CreateCommentDto createDto)
    {
        try
        {
            // Ensure reportId matches
            createDto.ReportId = reportId;
            
            var comment = await _commentService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CommentDto>> Update(Guid id, [FromBody] UpdateCommentDto updateDto, [FromQuery] Guid strategistId)
    {
        try
        {
            var comment = await _commentService.UpdateAsync(id, strategistId, updateDto);
            return Ok(comment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id, [FromQuery] Guid strategistId)
    {
        try
        {
            var deleted = await _commentService.DeleteAsync(id, strategistId);
            if (!deleted)
                return NotFound($"Comment with ID {id} not found.");

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}


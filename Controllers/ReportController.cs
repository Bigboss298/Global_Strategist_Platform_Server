using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReportDto>> GetById(Guid id)
    {
        var report = await _reportService.GetByIdAsync(id);
        if (report == null)
            return NotFound($"Report with ID {id} not found.");

        return Ok(report);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetAll()
    {
        var reports = await _reportService.GetAllAsync();
        return Ok(reports);
    }

    [HttpGet("strategist/{strategistId}")]
    public async Task<ActionResult<IEnumerable<ReportFeedDto>>> GetByStrategistId(Guid strategistId)
    {
        try
        {
            var reports = await _reportService.GetByStrategistIdAsync(strategistId);
            return Ok(reports);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("field/{fieldId}")]
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetByFieldId(Guid fieldId)
    {
        try
        {
            var reports = await _reportService.GetByFieldIdAsync(fieldId);
            return Ok(reports);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetByProjectId(Guid projectId)
    {
        try
        {
            var reports = await _reportService.GetByProjectIdAsync(projectId);
            return Ok(reports);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ReportDto>>> GetByCategoryId(Guid categoryId)
    {
        try
        {
            var reports = await _reportService.GetByCategoryIdAsync(categoryId);
            return Ok(reports);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("feed")]
    public async Task<ActionResult<IEnumerable<ReportFeedDto>>> GetFeed([FromQuery] Guid? userId = null)
    {
        var feed = await _reportService.GetFeedAsync(userId);
        return Ok(feed);
    }

    [HttpGet("feed/project/{projectId}")]
    public async Task<ActionResult<IEnumerable<ReportFeedDto>>> GetFeedByProject(Guid projectId, [FromQuery] Guid? userId = null)
    {
        try
        {
            var feed = await _reportService.GetFeedByProjectAsync(projectId, userId);
            return Ok(feed);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<ReportDto>> Create([FromBody] CreateReportDto createDto)
    {
        try
        {
            var report = await _reportService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = report.Id }, report);
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
    public async Task<ActionResult<ReportDto>> Update(Guid id, [FromBody] UpdateReportDto updateDto)
    {
        try
        {
            var report = await _reportService.UpdateAsync(id, updateDto);
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _reportService.DeleteAsync(id);
        if (!deleted)
            return NotFound($"Report with ID {id} not found.");

        return NoContent();
    }
    [HttpGet("paged")]
    public async Task<ActionResult<PagedResult<ReportDto>>> GetAllPaged([FromQuery] PaginationRequest request)
    {
        var pagedReports = await _reportService.GetAllPagedAsync(request);
        return Ok(pagedReports);
    }

    [HttpGet("strategist/{strategistId}/paged")]
    public async Task<ActionResult<PagedResult<ReportDto>>> GetByStrategistIdPaged(
        Guid strategistId,
        [FromQuery] PaginationRequest request)
    {
        try
        {
            var pagedReports = await _reportService.GetByStrategistIdPagedAsync(strategistId, request);
            return Ok(pagedReports);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("field/{fieldId}/paged")]
    public async Task<ActionResult<PagedResult<ReportDto>>> GetByFieldIdPaged(
        Guid fieldId,
        [FromQuery] PaginationRequest request)
    {
        try
        {
            var pagedReports = await _reportService.GetByFieldIdPagedAsync(fieldId, request);
            return Ok(pagedReports);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("project/{projectId}/paged")]
    public async Task<ActionResult<PagedResult<ReportDto>>> GetByProjectIdPaged(
        Guid projectId,
        [FromQuery] PaginationRequest request)
    {
        try
        {
            var pagedReports = await _reportService.GetByProjectIdPagedAsync(projectId, request);
            return Ok(pagedReports);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("category/{categoryId}/paged")]
    public async Task<ActionResult<PagedResult<ReportDto>>> GetByCategoryIdPaged(
        Guid categoryId,
        [FromQuery] PaginationRequest request)
    {
        try
        {
            var pagedReports = await _reportService.GetByCategoryIdPagedAsync(categoryId, request);
            return Ok(pagedReports);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("feed/paged")]
    public async Task<ActionResult<PagedResult<ReportFeedDto>>> GetFeedPaged(
        [FromQuery] Guid? userId = null,
        [FromQuery] PaginationRequest? request = null)
    {
        request ??= new PaginationRequest();
        var pagedFeed = await _reportService.GetFeedPagedAsync(userId, request);
        return Ok(pagedFeed);
    }
}


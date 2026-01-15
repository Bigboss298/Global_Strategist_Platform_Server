using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project == null)
            return NotFound($"Project with ID {id} not found.");

        return Ok(project);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAll()
    {
        var projects = await _projectService.GetAllAsync();
        return Ok(projects);
    }

    [HttpGet("with-fields")]
    public async Task<ActionResult<IEnumerable<ProjectWithFieldsDto>>> GetAllWithFields()
    {
        var projects = await _projectService.GetAllWithFieldsAsync();
        return Ok(projects);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PagedResult<ProjectDto>>> GetAllPaged([FromQuery] PaginationRequest request)
    {
        var pagedProjects = await _projectService.GetAllPagedAsync(request);
        return Ok(pagedProjects);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetByCategoryId(Guid categoryId)
    {
        try
        {
            var projects = await _projectService.GetByCategoryIdAsync(categoryId);
            return Ok(projects);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("category/{categoryId}/paged")]
    public async Task<ActionResult<PagedResult<ProjectDto>>> GetByCategoryIdPaged(
        Guid categoryId, 
        [FromQuery] PaginationRequest request)
    {
        try
        {
            var pagedProjects = await _projectService.GetByCategoryIdPagedAsync(categoryId, request);
            return Ok(pagedProjects);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectDto createDto)
    {
        try
        {
            var project = await _projectService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
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

    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectDto>> Update(Guid id, [FromBody] UpdateProjectDto updateDto)
    {
        try
        {
            var project = await _projectService.UpdateAsync(id, updateDto);
            return Ok(project);
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
        var deleted = await _projectService.DeleteAsync(id);
        if (!deleted)
            return NotFound($"Project with ID {id} not found.");

        return NoContent();
    }
}


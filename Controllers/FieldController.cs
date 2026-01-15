using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FieldController : ControllerBase
{
    private readonly IFieldService _fieldService;

    public FieldController(IFieldService fieldService)
    {
        _fieldService = fieldService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FieldDto>> GetById(Guid id)
    {
        var field = await _fieldService.GetByIdAsync(id);
        if (field == null)
            return NotFound($"Field with ID {id} not found.");

        return Ok(field);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FieldDto>>> GetAll()
    {
        var fields = await _fieldService.GetAllAsync();
        return Ok(fields);
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<FieldDto>>> GetByProjectId(Guid projectId)
    {
        try
        {
            var fields = await _fieldService.GetByProjectIdAsync(projectId);
            return Ok(fields);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<FieldDto>> Create([FromBody] CreateFieldDto createDto)
    {
        try
        {
            var field = await _fieldService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = field.Id }, field);
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
    public async Task<ActionResult<FieldDto>> Update(Guid id, [FromBody] UpdateFieldDto updateDto)
    {
        try
        {
            var field = await _fieldService.UpdateAsync(id, updateDto);
            return Ok(field);
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
        var deleted = await _fieldService.DeleteAsync(id);
        if (!deleted)
            return NotFound($"Field with ID {id} not found.");

        return NoContent();
    }
}


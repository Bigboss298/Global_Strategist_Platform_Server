using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;

namespace Global_Strategist_Platform_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound($"User with ID {id} not found.");

        return Ok(user);
    }

    [Authorize]
    [HttpGet("email/{email}")]
    public async Task<ActionResult<UserDto>> GetByEmail(string email)
    {
        try
        {
            var user = await _userService.GetByEmailAsync(email);
            if (user == null)
                return NotFound($"User with email {email} not found.");

            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PagedResult<UserDto>>> GetAllPaged([FromQuery] PaginationRequest request)
    {
        var pagedUsers = await _userService.GetAllPagedAsync(request);
        return Ok(pagedUsers);
    }


    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(Guid id, [FromForm] UpdateUserDto updateDto)
    {
        try
        {
            var user = await _userService.UpdateAsync(id, updateDto);
            return Ok(user);
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

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _userService.DeleteAsync(id);
        if (!deleted)
            return NotFound($"User with ID {id} not found.");

        return NoContent();
    }

    // Admin endpoint for badge management
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/badge")]
    public async Task<ActionResult<UserDto>> UpdateBadge(Guid id, [FromBody] UpdateBadgeDto updateBadgeDto)
    {
        try
        {
            var user = await _userService.UpdateBadgeAsync(id, updateBadgeDto);
            return Ok(user);
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

    // Admin endpoint to get all users with their badge status
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/badge-management")]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsersForBadgeManagement([FromQuery] PaginationRequest request)
    {
        var users = await _userService.GetAllPagedAsync(request);
        return Ok(users);
    }
}

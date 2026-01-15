using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;
using System.Security.Claims;

namespace Global_Strategist_Platform_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CorporateController : ControllerBase
{
    private readonly ICorporateService _corporateService;

    public CorporateController(ICorporateService corporateService)
    {
        _corporateService = corporateService;
    }

    [HttpGet("paged")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllPaged([FromQuery] PaginationRequest request)
    {
        try
        {
            var pagedCorporates = await _corporateService.GetAllPagedAsync(request);
            return Ok(pagedCorporates);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
        }
    }

    [HttpPost("{id}/purchase-slots")]
    [Authorize(Roles = "CorporateAdmin")]
    public async Task<IActionResult> PurchaseSlots(Guid id, [FromBody] PurchaseSlotsRequest request)
    {
        try
        {
            var response = await _corporateService.PurchaseSlotsAsync(id, request);
            return Ok(new { success = true, data = response });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
        }
    }

    [HttpGet("{id}/dashboard")]
    [Authorize(Roles = "CorporateAdmin,CorporateTeam")]
    public async Task<IActionResult> GetDashboard(Guid id)
    {
        try
        {
            var dashboard = await _corporateService.GetDashboardAsync(id);
            return Ok(new { success = true, data = dashboard });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
        }
    }
}
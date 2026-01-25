using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Global_Strategist_Platform_Server.Interface.Services;
using Global_Strategist_Platform_Server.Model.DTOs;
using System.Security.Claims;

namespace Global_Strategist_Platform_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("register/individual")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromForm] RegisterIndividualRequest dto)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var response = await _authService.RegisterIndividualAsync(dto, ipAddress);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("register/corporate")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCorporate([FromBody] RegisterCorporateRequest dto)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var response = await _authService.RegisterCorporateAsync(dto, ipAddress);
            
            return Ok(new 
            { 
                success = true, 
                message = "Corporate account created successfully. Representative is now logged in.",
                data = response 
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred during registration." });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var response = await _authService.LoginAsync(dto, ipAddress);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshRequestDto request)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var response = await _authService.RefreshTokenAsync(request.Token, ipAddress);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<ActionResult> RevokeToken([FromBody] RevokeTokenRequestDto request)
    {
        try
        {
            var ipAddress = GetIpAddress();
            await _authService.RevokeRefreshTokenAsync(request.Token, ipAddress);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            // Debug: return all claims for troubleshooting
            var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Unauthorized(new { 
                message = "Invalid token: User ID claim not found.",
                claims = allClaims 
            });
        }

        var user = await _authService.GetCurrentUserAsync(userId);
        if (user == null)
            return NotFound("User not found.");

        return Ok(user);
    }
    
    [HttpGet("test-token")]
    [Authorize]
    public ActionResult TestToken()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(new { 
            message = "Token is valid!",
            claims = claims,
            userId = User.FindFirst("sub")?.Value,
            email = User.FindFirst("email")?.Value,
            name = User.FindFirst("name")?.Value,
            role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }

    [HttpPost("invite-team-member")]
    [Authorize(Roles = "CorporateAdmin")]
    public async Task<IActionResult> InviteTeamMember([FromBody] InviteTeamMemberRequest dto)
    {
        try
        {
            var invite = await _authService.InviteTeamMemberAsync(dto);
            return Ok(new 
            { 
                success = true, 
                message = "Invitation sent successfully.",
                data = new 
                {
                    inviteId = invite.Id,
                    email = invite.Email,
                    token = invite.Token,
                    expiresAt = invite.ExpiresAt
                }
            });
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
            return StatusCode(500, new { success = false, message = "An error occurred while sending the invitation." });
        }
    }

    [HttpPost("register/team-member")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterTeamMember([FromForm] RegisterTeamMemberRequest dto)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var response = await _authService.RegisterTeamMemberAsync(dto, ipAddress);
            
            return Ok(new 
            { 
                success = true, 
                message = "Team member registered successfully.",
                data = response 
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "An error occurred during registration." });
        }
    }

    private string GetIpAddress()
    {
        // Check for forwarded IP (for proxies/load balancers)
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',');
            return ips[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}


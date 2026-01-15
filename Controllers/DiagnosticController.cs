using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Global_Strategist_Platform_Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticController : ControllerBase
{
    [HttpGet("check-auth")]
    [Authorize]
    public ActionResult CheckAuth()
    {
        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
            AuthenticationType = User.Identity?.AuthenticationType,
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
            UserId = User.FindFirst("sub")?.Value,
            Email = User.FindFirst("email")?.Value,
            Name = User.FindFirst("name")?.Value,
            Role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }
}


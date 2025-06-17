using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace JewelryStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            message = "API is running with latest changes"
        });
    }

    [HttpGet("auth")]
    [Authorize]
    public IActionResult TestAuth()
    {
        Console.WriteLine("=== TEST AUTH ENDPOINT ===");
        Console.WriteLine($"User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
        Console.WriteLine($"User.Identity.Name: {User.Identity?.Name}");
        Console.WriteLine("All claims:");
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"  {claim.Type}: {claim.Value}");
        }
        Console.WriteLine("=== END TEST AUTH ===");

        return Ok(new
        {
            isAuthenticated = User.Identity?.IsAuthenticated,
            name = User.Identity?.Name,
            claims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList()
        });
    }
}
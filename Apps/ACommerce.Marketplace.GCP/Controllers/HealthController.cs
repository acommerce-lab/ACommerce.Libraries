using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.Marketplace.GCP.Controllers;

/// <summary>
/// Health check controller for load balancers and monitoring
/// </summary>
[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class HealthzController : ControllerBase
{
    /// <summary>
    /// Simple health check endpoint for load balancers
    /// </summary>
    [HttpGet("/healthz")]
    public IActionResult Get()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "ACommerce Marketplace API",
            Version = "1.0.0",
            Timestamp = DateTime.UtcNow
        });
    }
}

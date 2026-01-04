using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ACommerce.Marketing.Analytics.Controllers;

[ApiController]
[Route("api/marketing/[controller]")]
public class AttributionController : ControllerBase
{
    private readonly IAttributionService _attribution;
    private readonly ILogger<AttributionController> _logger;

    public AttributionController(IAttributionService attribution, ILogger<AttributionController> logger)
    {
        _attribution = attribution;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Capture([FromBody] AttributionCaptureRequest request)
    {
        if (request == null)
            return BadRequest();

        // enrich with request info if missing
        if (string.IsNullOrEmpty(request.ReferrerUrl) && Request.Headers.ContainsKey("Referer"))
            request.ReferrerUrl = Request.Headers["Referer"].ToString();

        if (string.IsNullOrEmpty(request.DeviceType))
            request.DeviceType = Request.Headers["User-Agent"].ToString();

        // attempt to set session id from cookie/header if not provided
        if (string.IsNullOrEmpty(request.SessionId))
        {
            if (Request.Cookies.TryGetValue("ashare_session", out var sid))
                request.SessionId = sid;
            else if (Request.Headers.TryGetValue("X-Session-Id", out var headerSid))
                request.SessionId = headerSid.ToString();
        }

        try
        {
            var attribution = await _attribution.CaptureAttributionAsync(request);
            return Ok(new { success = true, attributionId = attribution.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Attribution] Failed to capture attribution");
            return StatusCode(500, new { success = false });
        }
    }
}
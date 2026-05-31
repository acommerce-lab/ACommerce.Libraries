using ACommerce.AppConfig.Contracts;
using ACommerce.AppConfig.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACommerce.AppConfig.Api.Controllers;

/// <summary>
/// نقطة العميل العامة — يجلب الـ snapshot الكاملة.
/// مدعومة بـ ETag (If-None-Match) لتقليل النقل.
/// </summary>
[ApiController]
[Route("api/appconfig")]
[AllowAnonymous]
public class AppConfigController(IAppConfigService service) : ControllerBase
{
    /// <summary>
    /// GET /api/appconfig/snapshot?lang=ar&amp;platform=android&amp;version=1.16
    /// يعيد 200 + ETag، أو 304 إذا أرسل العميل If-None-Match مطابق.
    /// </summary>
    [HttpGet("snapshot")]
    [ProducesResponseType(typeof(AppConfigSnapshot), 200)]
    [ProducesResponseType(304)]
    public async Task<IActionResult> GetSnapshot(
        [FromQuery(Name = "lang")] string lang = "ar",
        [FromQuery] string? platform = null,
        [FromQuery] string? version = null,
        CancellationToken ct = default)
    {
        var snap = await service.GetSnapshotAsync(lang, platform, version, ct);
        var etag = $"\"{snap.Version}\"";

        // 304 if client already has the latest version
        if (Request.Headers.TryGetValue("If-None-Match", out var existing)
            && existing.ToString() == etag)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers["ETag"] = etag;
        Response.Headers["Cache-Control"] = "private, max-age=300"; // 5 min client-side
        return Ok(snap);
    }
}

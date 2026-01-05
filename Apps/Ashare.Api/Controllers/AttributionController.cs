using ACommerce.Marketing.Analytics.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ashare.Api.Controllers;

/// <summary>
/// Controller لالتقاط بيانات الإسناد التسويقي (Attribution)
/// يستقبل بيانات النقرات من الحملات الإعلانية ويحفظها للتتبع
/// </summary>
[ApiController]
[Route("api/marketing/[controller]")]
public class AttributionController : ControllerBase
{
    private readonly IAttributionService _attributionService;
    private readonly ILogger<AttributionController> _logger;

    public AttributionController(
        IAttributionService attributionService,
        ILogger<AttributionController> logger)
    {
        _attributionService = attributionService;
        _logger = logger;
    }

    /// <summary>
    /// التقاط بيانات الإسناد من الـ JavaScript أو التطبيق
    /// POST /api/marketing/attribution
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Capture([FromBody] AttributionCaptureRequest request)
    {
        if (request == null)
            return BadRequest(new { success = false, error = "Request body is required" });

        try
        {
            // إثراء البيانات من الطلب إذا لم تكن موجودة
            if (string.IsNullOrEmpty(request.ReferrerUrl) && Request.Headers.ContainsKey("Referer"))
                request.ReferrerUrl = Request.Headers["Referer"].ToString();

            if (string.IsNullOrEmpty(request.DeviceType))
                request.DeviceType = Request.Headers.UserAgent.ToString();

            // محاولة الحصول على SessionId من الكوكي أو الهيدر
            if (string.IsNullOrEmpty(request.SessionId))
            {
                if (Request.Cookies.TryGetValue("ashare_session", out var cookieSession))
                    request.SessionId = cookieSession;
                else if (Request.Headers.TryGetValue("X-Session-Id", out var headerSession))
                    request.SessionId = headerSession.ToString();
            }

            // إنشاء SessionId جديد إذا لم يكن موجوداً
            if (string.IsNullOrEmpty(request.SessionId))
                request.SessionId = Guid.NewGuid().ToString("N");

            _logger.LogInformation(
                "[Attribution] Capturing: SessionId={SessionId}, Source={Source}, Campaign={Campaign}, ClickId={ClickId}",
                request.SessionId, request.UtmSource, request.UtmCampaign, request.ClickId);

            var attribution = await _attributionService.CaptureAttributionAsync(request);

            return Ok(new
            {
                success = true,
                attributionId = attribution.Id,
                sessionId = request.SessionId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Attribution] Failed to capture attribution");
            return StatusCode(500, new { success = false, error = "Failed to capture attribution" });
        }
    }

    /// <summary>
    /// ربط المستخدم بالإسناد بعد تسجيل الدخول
    /// POST /api/marketing/attribution/associate
    /// </summary>
    [HttpPost("associate")]
    public async Task<IActionResult> AssociateUser([FromBody] AssociateUserRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.UserId))
            return BadRequest(new { success = false, error = "UserId is required" });

        try
        {
            if (request.AttributionId.HasValue)
            {
                await _attributionService.AssociateUserWithAttributionAsync(
                    request.AttributionId.Value, request.UserId);
            }
            else if (!string.IsNullOrEmpty(request.SessionId))
            {
                var attribution = await _attributionService.GetAttributionBySessionAsync(request.SessionId);
                if (attribution != null)
                {
                    await _attributionService.AssociateUserWithAttributionAsync(
                        attribution.Id, request.UserId);
                }
            }

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Attribution] Failed to associate user");
            return StatusCode(500, new { success = false, error = "Failed to associate user" });
        }
    }

    /// <summary>
    /// الحصول على بيانات الإسناد للجلسة
    /// GET /api/marketing/attribution/{sessionId}
    /// </summary>
    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetBySession(string sessionId)
    {
        try
        {
            var attribution = await _attributionService.GetAttributionBySessionAsync(sessionId);
            if (attribution == null)
                return NotFound(new { success = false, error = "Attribution not found" });

            return Ok(new
            {
                success = true,
                attribution = new
                {
                    attribution.Id,
                    attribution.SessionId,
                    attribution.Platform,
                    attribution.UtmSource,
                    attribution.UtmMedium,
                    attribution.UtmCampaign,
                    attribution.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Attribution] Failed to get attribution");
            return StatusCode(500, new { success = false, error = "Failed to get attribution" });
        }
    }
}

/// <summary>
/// طلب ربط المستخدم بالإسناد
/// </summary>
public class AssociateUserRequest
{
    public string? SessionId { get; set; }
    public Guid? AttributionId { get; set; }
    public required string UserId { get; set; }
}

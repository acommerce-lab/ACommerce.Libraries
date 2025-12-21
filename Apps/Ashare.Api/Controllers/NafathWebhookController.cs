using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.AspNetCore.NafathWH.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Ashare.Api.Controllers;

/// <summary>
/// متحكم Webhook لنفاذ
/// يستقبل الاستدعاءات من خدمة نفاذ عند اكتمال المصادقة
/// </summary>
[ApiController]
[Route("api/auth")]
public class NafathWebhookController : NafathWebhookControllerBase
{
    public NafathWebhookController(
        ITwoFactorAuthenticationProvider nafathProvider,
        IConfiguration configuration,
        ILogger<NafathWebhookController> logger)
        : base(nafathProvider, configuration, logger)
    {
    }
}

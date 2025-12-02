using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.AspNetCore.NafathWH.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Ashare.Api.Controllers;

/// <summary>
/// متحكم Webhook لنفاذ
/// يستقبل الاستدعاءات من خدمة نفاذ عند اكتمال المصادقة
/// </summary>
[ApiController]
[Route("api/nafath/webhook")]
public class NafathWebhookController : NafathWebhookControllerBase
{
    public NafathWebhookController(
        ITwoFactorAuthenticationProvider nafathProvider,
        ILogger<NafathWebhookController> logger)
        : base(nafathProvider, logger)
    {
    }
}

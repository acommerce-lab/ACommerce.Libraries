using ACommerce.Subscriptions.DTOs;
using ACommerce.Subscriptions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ACommerce.Subscriptions.Api.Controllers;

/// <summary>
/// API للاشتراكات
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    #region Plans - الباقات

    /// <summary>الحصول على جميع الباقات</summary>
    [HttpGet("plans")]
    public async Task<ActionResult<List<SubscriptionPlanDto>>> GetPlans(CancellationToken ct)
    {
        var plans = await _subscriptionService.GetPlansAsync(ct);
        return Ok(plans);
    }

    /// <summary>الحصول على باقة بواسطة المعرف</summary>
    [HttpGet("plans/{planId:guid}")]
    public async Task<ActionResult<SubscriptionPlanDto>> GetPlanById(Guid planId, CancellationToken ct)
    {
        var plan = await _subscriptionService.GetPlanByIdAsync(planId, ct);
        if (plan == null) return NotFound();
        return Ok(plan);
    }

    /// <summary>الحصول على باقة بواسطة الـ Slug</summary>
    [HttpGet("plans/by-slug/{slug}")]
    public async Task<ActionResult<SubscriptionPlanDto>> GetPlanBySlug(string slug, CancellationToken ct)
    {
        var plan = await _subscriptionService.GetPlanBySlugAsync(slug, ct);
        if (plan == null) return NotFound();
        return Ok(plan);
    }

    /// <summary>إنشاء باقة جديدة (للمسؤولين)</summary>
    [HttpPost("plans")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubscriptionPlanDto>> CreatePlan([FromBody] CreateSubscriptionPlanDto dto, CancellationToken ct)
    {
        var plan = await _subscriptionService.CreatePlanAsync(dto, ct);
        return CreatedAtAction(nameof(GetPlanById), new { planId = plan.Id }, plan);
    }

    /// <summary>تحديث باقة (للمسؤولين)</summary>
    [HttpPut("plans/{planId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SubscriptionPlanDto>> UpdatePlan(Guid planId, [FromBody] UpdateSubscriptionPlanDto dto, CancellationToken ct)
    {
        if (planId != dto.Id) return BadRequest("Plan ID mismatch");
        var plan = await _subscriptionService.UpdatePlanAsync(dto, ct);
        if (plan == null) return NotFound();
        return Ok(plan);
    }

    /// <summary>حذف باقة (للمسؤولين)</summary>
    [HttpDelete("plans/{planId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeletePlan(Guid planId, CancellationToken ct)
    {
        var result = await _subscriptionService.DeletePlanAsync(planId, ct);
        if (!result) return NotFound();
        return NoContent();
    }

    #endregion

    #region Subscriptions - الاشتراكات

    /// <summary>الحصول على اشتراك المزود</summary>
    [HttpGet("vendor/{vendorId:guid}")]
    [Authorize]
    public async Task<ActionResult<SubscriptionDto>> GetVendorSubscription(Guid vendorId, CancellationToken ct)
    {
        var subscription = await _subscriptionService.GetVendorSubscriptionAsync(vendorId, ct);
        if (subscription == null) return NotFound();
        return Ok(subscription);
    }

    /// <summary>الحصول على اشتراك بواسطة المعرف</summary>
    [HttpGet("{subscriptionId:guid}")]
    [Authorize]
    public async Task<ActionResult<SubscriptionDto>> GetSubscriptionById(Guid subscriptionId, CancellationToken ct)
    {
        var subscription = await _subscriptionService.GetSubscriptionByIdAsync(subscriptionId, ct);
        if (subscription == null) return NotFound();
        return Ok(subscription);
    }

    /// <summary>إنشاء اشتراك جديد</summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<SubscriptionDto>> CreateSubscription([FromBody] CreateSubscriptionDto dto, CancellationToken ct)
    {
        try
        {
            // استخراج معرف المستخدم من التوكن كـ VendorId
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var vendorId))
            {
                return Unauthorized("لم يتم العثور على معرف المستخدم");
            }

            // تعيين VendorId من التوكن
            dto.VendorId = vendorId;

            var subscription = await _subscriptionService.CreateSubscriptionAsync(dto, ct);
            return CreatedAtAction(nameof(GetSubscriptionById), new { subscriptionId = subscription.Id }, subscription);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>تغيير الباقة</summary>
    [HttpPost("{subscriptionId:guid}/change-plan")]
    [Authorize]
    public async Task<ActionResult<SubscriptionDto>> ChangePlan(Guid subscriptionId, [FromBody] ChangePlanDto dto, CancellationToken ct)
    {
        var subscription = await _subscriptionService.ChangePlanAsync(subscriptionId, dto, ct);
        if (subscription == null) return NotFound();
        return Ok(subscription);
    }

    /// <summary>إلغاء الاشتراك</summary>
    [HttpPost("{subscriptionId:guid}/cancel")]
    [Authorize]
    public async Task<ActionResult> CancelSubscription(Guid subscriptionId, [FromBody] CancelSubscriptionDto dto, CancellationToken ct)
    {
        var result = await _subscriptionService.CancelSubscriptionAsync(subscriptionId, dto, ct);
        if (!result) return NotFound();
        return Ok(new { message = "تم إلغاء الاشتراك بنجاح" });
    }

    /// <summary>تجديد الاشتراك</summary>
    [HttpPost("{subscriptionId:guid}/renew")]
    [Authorize]
    public async Task<ActionResult<SubscriptionDto>> RenewSubscription(Guid subscriptionId, [FromBody] RenewSubscriptionDto dto, CancellationToken ct)
    {
        var subscription = await _subscriptionService.RenewSubscriptionAsync(subscriptionId, dto, ct);
        if (subscription == null) return NotFound();
        return Ok(subscription);
    }

    /// <summary>تفعيل الاشتراك بعد الدفع</summary>
    [HttpPost("{subscriptionId:guid}/activate")]
    [Authorize]
    public async Task<ActionResult<SubscriptionDto>> ActivateSubscription(Guid subscriptionId, [FromBody] ActivateSubscriptionRequest? request, CancellationToken ct)
    {
        var subscription = await _subscriptionService.ActivateSubscriptionAsync(subscriptionId, request?.PaymentId, ct);
        if (subscription == null) return NotFound();
        return Ok(subscription);
    }

    /// <summary>الحصول على ملخص الاشتراك</summary>
    [HttpGet("vendor/{vendorId:guid}/summary")]
    [Authorize]
    public async Task<ActionResult<SubscriptionSummaryDto>> GetSubscriptionSummary(Guid vendorId, CancellationToken ct)
    {
        var summary = await _subscriptionService.GetSubscriptionSummaryAsync(vendorId, ct);
        if (summary == null) return NotFound();
        return Ok(summary);
    }

    #endregion

    #region Usage - الاستخدام

    /// <summary>التحقق من إمكانية إضافة عرض</summary>
    [HttpGet("vendor/{vendorId:guid}/can-add-listing")]
    [Authorize]
    public async Task<ActionResult<CanAddListingResult>> CanAddListing(Guid vendorId, CancellationToken ct)
    {
        var result = await _subscriptionService.CanAddListingAsync(vendorId, ct);
        return Ok(result);
    }

    /// <summary>الحصول على إحصائيات الاستخدام</summary>
    [HttpGet("vendor/{vendorId:guid}/usage")]
    [Authorize]
    public async Task<ActionResult<VendorUsageStatsDto>> GetUsageStats(Guid vendorId, CancellationToken ct)
    {
        var stats = await _subscriptionService.GetUsageStatsAsync(vendorId, ct);
        if (stats == null) return NotFound();
        return Ok(stats);
    }

    /// <summary>حساب العمولة</summary>
    [HttpGet("vendor/{vendorId:guid}/calculate-commission")]
    [Authorize]
    public async Task<ActionResult<CommissionCalculationDto>> CalculateCommission(Guid vendorId, [FromQuery] decimal amount, CancellationToken ct)
    {
        var result = await _subscriptionService.CalculateCommissionAsync(vendorId, amount, ct);
        return Ok(result);
    }

    #endregion

    #region Invoices - الفواتير

    /// <summary>الحصول على فواتير المزود</summary>
    [HttpGet("vendor/{vendorId:guid}/invoices")]
    [Authorize]
    public async Task<ActionResult<List<InvoiceSummaryDto>>> GetVendorInvoices(Guid vendorId, CancellationToken ct)
    {
        var invoices = await _subscriptionService.GetVendorInvoicesAsync(vendorId, ct);
        return Ok(invoices);
    }

    /// <summary>الحصول على فاتورة</summary>
    [HttpGet("invoices/{invoiceId:guid}")]
    [Authorize]
    public async Task<ActionResult<SubscriptionInvoiceDto>> GetInvoice(Guid invoiceId, CancellationToken ct)
    {
        var invoice = await _subscriptionService.GetInvoiceByIdAsync(invoiceId, ct);
        if (invoice == null) return NotFound();
        return Ok(invoice);
    }

    #endregion
}

/// <summary>طلب تفعيل الاشتراك</summary>
public class ActivateSubscriptionRequest
{
    public string? PaymentId { get; set; }
}

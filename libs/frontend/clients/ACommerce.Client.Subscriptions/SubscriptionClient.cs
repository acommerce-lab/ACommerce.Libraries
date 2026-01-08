using ACommerce.Client.Core.Http;
using ACommerce.Subscriptions.DTOs;
using ACommerce.Subscriptions.Enums;

namespace ACommerce.Client.Subscriptions;

/// <summary>
/// عميل API الاشتراكات
/// </summary>
public sealed class SubscriptionClient
{
    private readonly IApiClient _httpClient;
    private const string ServiceName = "Marketplace";
    private const string BasePath = "/api/subscriptions";

    public SubscriptionClient(IApiClient httpClient)
    {
        _httpClient = httpClient;
    }

    #region Plans - الباقات

    /// <summary>الحصول على جميع الباقات</summary>
    public async Task<List<SubscriptionPlanDto>?> GetPlansAsync(CancellationToken ct = default)
    {
        return await _httpClient.GetAsync<List<SubscriptionPlanDto>>(
            ServiceName,
            $"{BasePath}/plans",
            ct);
    }

    /// <summary>الحصول على باقة بواسطة المعرف</summary>
    public async Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid planId, CancellationToken ct = default)
    {
        return await _httpClient.GetAsync<SubscriptionPlanDto>(
            ServiceName,
            $"{BasePath}/plans/{planId}",
            ct);
    }

    /// <summary>الحصول على باقة بواسطة الـ Slug</summary>
    public async Task<SubscriptionPlanDto?> GetPlanBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _httpClient.GetAsync<SubscriptionPlanDto>(
            ServiceName,
            $"{BasePath}/plans/by-slug/{slug}",
            ct);
    }

    #endregion

    #region Subscriptions - الاشتراكات

    /// <summary>الحصول على اشتراك المزود</summary>
    public async Task<SubscriptionDto?> GetVendorSubscriptionAsync(Guid vendorId, CancellationToken ct = default)
    {
        return await _httpClient.GetAsync<SubscriptionDto>(
            ServiceName,
            $"{BasePath}/vendor/{vendorId}",
            ct);
    }

    /// <summary>الحصول على اشتراك بواسطة المعرف</summary>
    public async Task<SubscriptionDto?> GetSubscriptionByIdAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        return await _httpClient.GetAsync<SubscriptionDto>(
            ServiceName,
            $"{BasePath}/{subscriptionId}",
            ct);
    }

    /// <summary>إنشاء اشتراك جديد</summary>
    public async Task<SubscriptionDto?> CreateSubscriptionAsync(CreateSubscriptionDto dto, CancellationToken ct = default)
    {
        return await _httpClient.PostAsync<CreateSubscriptionDto, SubscriptionDto>(
            ServiceName,
            BasePath,
            dto,
            ct);
    }

    /// <summary>تغيير الباقة</summary>
    public async Task<SubscriptionDto?> ChangePlanAsync(Guid subscriptionId, ChangePlanDto dto, CancellationToken ct = default)
    {
        return await _httpClient.PostAsync<ChangePlanDto, SubscriptionDto>(
            ServiceName,
            $"{BasePath}/{subscriptionId}/change-plan",
            dto,
            ct);
    }

    /// <summary>إلغاء الاشتراك</summary>
    public async Task<bool> CancelSubscriptionAsync(Guid subscriptionId, CancelSubscriptionDto dto, CancellationToken ct = default)
    {
        try
        {
            await _httpClient.PostAsync(
                ServiceName,
                $"{BasePath}/{subscriptionId}/cancel",
                dto,
                ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>تجديد الاشتراك</summary>
    public async Task<SubscriptionDto?> RenewSubscriptionAsync(Guid subscriptionId, RenewSubscriptionDto dto, CancellationToken ct = default)
    {
        return await _httpClient.PostAsync<RenewSubscriptionDto, SubscriptionDto>(
            ServiceName,
            $"{BasePath}/{subscriptionId}/renew",
            dto,
            ct);
    }

    /// <summary>تفعيل الاشتراك بعد الدفع</summary>
    public async Task<SubscriptionDto?> ActivateSubscriptionAsync(Guid subscriptionId, string? paymentId = null, CancellationToken ct = default)
    {
        return await _httpClient.PostAsync<object, SubscriptionDto>(
            ServiceName,
            $"{BasePath}/{subscriptionId}/activate",
            new { PaymentId = paymentId },
            ct);
    }

    /// <summary>الحصول على ملخص الاشتراك</summary>
    public async Task<SubscriptionSummaryDto?> GetSubscriptionSummaryAsync(Guid vendorId, CancellationToken ct = default)
    {
        return await _httpClient.GetAsync<SubscriptionSummaryDto>(
            ServiceName,
            $"{BasePath}/vendor/{vendorId}/summary",
            ct);
    }

    #endregion

    #region Usage - الاستخدام

    /// <summary>التحقق من إمكانية إضافة عرض</summary>
    public async Task<CanAddListingResult?> CanAddListingAsync(Guid vendorId, CancellationToken ct = default)
    {
        return await _httpClient.GetAsync<CanAddListingResult>(
            ServiceName,
            $"{BasePath}/vendor/{vendorId}/can-add-listing",
            ct);
    }

    /// <summary>التحقق من إمكانية إضافة عرض لفئة محددة</summary>
    public async Task<CanAddListingResult?> CanAddListingAsync(Guid vendorId, Guid categoryId, CancellationToken ct = default)
    {
        return await _httpClient.GetAsync<CanAddListingResult>(
            ServiceName,
            $"{BasePath}/vendor/{vendorId}/can-add-listing?categoryId={categoryId}",
            ct);
    }

    /// <summary>الحصول على إحصائيات الاستخدام</summary>
    public async Task<VendorUsageStatsDto?> GetUsageStatsAsync(Guid vendorId, CancellationToken ct = default)
    {
        return await _httpClient.GetAsync<VendorUsageStatsDto>(
            ServiceName,
            $"{BasePath}/vendor/{vendorId}/usage",
            ct);
    }

    /// <summary>حساب العمولة</summary>
    public async Task<CommissionCalculationDto?> CalculateCommissionAsync(Guid vendorId, decimal amount, CancellationToken ct = default)
    {
        return await _httpClient.GetAsync<CommissionCalculationDto>(
            ServiceName,
            $"{BasePath}/vendor/{vendorId}/calculate-commission?amount={amount}",
            ct);
    }

    #endregion

    #region Invoices - الفواتير

    /// <summary>الحصول على فواتير المزود</summary>
    public async Task<List<InvoiceSummaryDto>?> GetVendorInvoicesAsync(Guid vendorId, CancellationToken ct = default)
    {
        return await _httpClient.GetAsync<List<InvoiceSummaryDto>>(
            ServiceName,
            $"{BasePath}/vendor/{vendorId}/invoices",
            ct);
    }

    /// <summary>الحصول على فاتورة</summary>
    public async Task<SubscriptionInvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken ct = default)
    {
        return await _httpClient.GetAsync<SubscriptionInvoiceDto>(
            ServiceName,
            $"{BasePath}/invoices/{invoiceId}",
            ct);
    }

    #endregion
}

using ACommerce.Subscriptions.DTOs;
using ACommerce.Subscriptions.Entities;
using ACommerce.Subscriptions.Enums;

namespace ACommerce.Subscriptions.Services;

/// <summary>
/// خدمة إدارة الاشتراكات
/// </summary>
public interface ISubscriptionService
{
    #region Plans - الباقات

    /// <summary>الحصول على جميع الباقات النشطة</summary>
    Task<List<SubscriptionPlanDto>> GetPlansAsync(CancellationToken ct = default);

    /// <summary>الحصول على باقة بواسطة المعرف</summary>
    Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid planId, CancellationToken ct = default);

    /// <summary>الحصول على باقة بواسطة الـ Slug</summary>
    Task<SubscriptionPlanDto?> GetPlanBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>إنشاء باقة جديدة</summary>
    Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto, CancellationToken ct = default);

    /// <summary>تحديث باقة</summary>
    Task<SubscriptionPlanDto?> UpdatePlanAsync(UpdateSubscriptionPlanDto dto, CancellationToken ct = default);

    /// <summary>حذف باقة</summary>
    Task<bool> DeletePlanAsync(Guid planId, CancellationToken ct = default);

    #endregion

    #region Subscriptions - الاشتراكات

    /// <summary>الحصول على اشتراك المزود</summary>
    Task<SubscriptionDto?> GetVendorSubscriptionAsync(Guid vendorId, CancellationToken ct = default);

    /// <summary>الحصول على اشتراك بواسطة المعرف</summary>
    Task<SubscriptionDto?> GetSubscriptionByIdAsync(Guid subscriptionId, CancellationToken ct = default);

    /// <summary>إنشاء اشتراك جديد</summary>
    Task<SubscriptionDto> CreateSubscriptionAsync(CreateSubscriptionDto dto, CancellationToken ct = default);

    /// <summary>ترقية/تخفيض الباقة</summary>
    Task<SubscriptionDto?> ChangePlanAsync(Guid subscriptionId, ChangePlanDto dto, CancellationToken ct = default);

    /// <summary>إلغاء الاشتراك</summary>
    Task<bool> CancelSubscriptionAsync(Guid subscriptionId, CancelSubscriptionDto dto, CancellationToken ct = default);

    /// <summary>تجديد الاشتراك</summary>
    Task<SubscriptionDto?> RenewSubscriptionAsync(Guid subscriptionId, RenewSubscriptionDto dto, CancellationToken ct = default);

    /// <summary>تفعيل الاشتراك بعد الدفع</summary>
    Task<SubscriptionDto?> ActivateSubscriptionAsync(Guid subscriptionId, string? paymentId = null, CancellationToken ct = default);

    /// <summary>الحصول على ملخص الاشتراك</summary>
    Task<SubscriptionSummaryDto?> GetSubscriptionSummaryAsync(Guid vendorId, CancellationToken ct = default);

    #endregion

    #region Usage - الاستخدام

    /// <summary>التحقق من إمكانية إضافة عرض</summary>
    Task<CanAddListingResult> CanAddListingAsync(Guid vendorId, CancellationToken ct = default);

    /// <summary>زيادة عداد العروض</summary>
    Task<bool> IncrementListingsCountAsync(Guid vendorId, CancellationToken ct = default);

    /// <summary>إنقاص عداد العروض</summary>
    Task<bool> DecrementListingsCountAsync(Guid vendorId, CancellationToken ct = default);

    /// <summary>تحديث استخدام التخزين</summary>
    Task<bool> UpdateStorageUsageAsync(Guid vendorId, decimal storageMB, CancellationToken ct = default);

    /// <summary>الحصول على إحصائيات الاستخدام</summary>
    Task<VendorUsageStatsDto?> GetUsageStatsAsync(Guid vendorId, CancellationToken ct = default);

    /// <summary>حساب العمولة</summary>
    Task<CommissionCalculationDto> CalculateCommissionAsync(Guid vendorId, decimal amount, CancellationToken ct = default);

    #endregion

    #region Invoices - الفواتير

    /// <summary>الحصول على فواتير المزود</summary>
    Task<List<InvoiceSummaryDto>> GetVendorInvoicesAsync(Guid vendorId, CancellationToken ct = default);

    /// <summary>الحصول على فاتورة بواسطة المعرف</summary>
    Task<SubscriptionInvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, CancellationToken ct = default);

    #endregion
}

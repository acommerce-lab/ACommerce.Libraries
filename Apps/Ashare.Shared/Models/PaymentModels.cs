using ACommerce.Subscriptions.Enums;

namespace Ashare.Shared.Models;

/// <summary>
/// نتيجة عملية الدفع
/// </summary>
public class PaymentResult
{
    public bool Success { get; set; }
    public bool Cancelled { get; set; }
    public string? OrderId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? Message { get; set; }
    public string? ErrorCode { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// طلب إنشاء دفعة اشتراك
/// </summary>
public class CreateSubscriptionPaymentRequest
{
    public Guid PlanId { get; set; }
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public string? CouponCode { get; set; }
    public string? ReturnUrl { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// استجابة إنشاء دفعة اشتراك
/// </summary>
public class CreateSubscriptionPaymentResponse
{
    public bool Success { get; set; }
    public Guid SubscriptionId { get; set; }
    public string? PaymentId { get; set; }
    public string? PaymentUrl { get; set; }
    public bool RequiresPayment { get; set; } = true;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "SAR";
    public string? Message { get; set; }
}

/// <summary>
/// طلب التحقق من حالة الدفع
/// </summary>
public class VerifyPaymentRequest
{
    public string PaymentId { get; set; } = string.Empty;
    public string? OrderId { get; set; }
    public string? TransactionId { get; set; }
}

/// <summary>
/// استجابة التحقق من الدفع
/// </summary>
public class VerifyPaymentResponse
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Completed, Failed, Cancelled
    public Guid? SubscriptionId { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public string? Message { get; set; }
}

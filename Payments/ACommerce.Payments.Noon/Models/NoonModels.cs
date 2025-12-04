using System.Text.Json.Serialization;

namespace ACommerce.Payments.Noon.Models;

#region Request Models

/// <summary>
/// طلب إنشاء عملية دفع
/// </summary>
public class NoonOrderRequest
{
	[JsonPropertyName("apiOperation")]
	public string ApiOperation { get; set; } = "INITIATE";

	[JsonPropertyName("order")]
	public NoonOrderInfo Order { get; set; } = new();

	[JsonPropertyName("configuration")]
	public NoonConfiguration Configuration { get; set; } = new();

	[JsonPropertyName("billing")]
	public NoonBillingInfo? Billing { get; set; }

	[JsonPropertyName("shipping")]
	public NoonShippingInfo? Shipping { get; set; }
}

/// <summary>
/// معلومات الطلب
/// </summary>
public class NoonOrderInfo
{
	[JsonPropertyName("reference")]
	public required string Reference { get; set; }

	[JsonPropertyName("amount")]
	public required decimal Amount { get; set; }

	[JsonPropertyName("currency")]
	public string Currency { get; set; } = "SAR";

	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("category")]
	public string Category { get; set; } = "pay";

	[JsonPropertyName("channel")]
	public string Channel { get; set; } = "web";
}

/// <summary>
/// إعدادات العملية
/// </summary>
public class NoonConfiguration
{
	[JsonPropertyName("returnUrl")]
	public required string ReturnUrl { get; set; }

	[JsonPropertyName("locale")]
	public string Locale { get; set; } = "ar";

	[JsonPropertyName("paymentAction")]
	public string PaymentAction { get; set; } = "SALE";

	[JsonPropertyName("tokenizeCc")]
	public bool? TokenizeCc { get; set; }
}

/// <summary>
/// معلومات الفوترة
/// </summary>
public class NoonBillingInfo
{
	[JsonPropertyName("contact")]
	public NoonContactInfo? Contact { get; set; }

	[JsonPropertyName("address")]
	public NoonAddressInfo? Address { get; set; }
}

/// <summary>
/// معلومات الشحن
/// </summary>
public class NoonShippingInfo
{
	[JsonPropertyName("contact")]
	public NoonContactInfo? Contact { get; set; }

	[JsonPropertyName("address")]
	public NoonAddressInfo? Address { get; set; }
}

/// <summary>
/// معلومات التواصل
/// </summary>
public class NoonContactInfo
{
	[JsonPropertyName("firstName")]
	public string? FirstName { get; set; }

	[JsonPropertyName("lastName")]
	public string? LastName { get; set; }

	[JsonPropertyName("phone")]
	public string? Phone { get; set; }

	[JsonPropertyName("email")]
	public string? Email { get; set; }
}

/// <summary>
/// معلومات العنوان
/// </summary>
public class NoonAddressInfo
{
	[JsonPropertyName("street")]
	public string? Street { get; set; }

	[JsonPropertyName("city")]
	public string? City { get; set; }

	[JsonPropertyName("stateProvince")]
	public string? StateProvince { get; set; }

	[JsonPropertyName("postalCode")]
	public string? PostalCode { get; set; }

	[JsonPropertyName("country")]
	public string Country { get; set; } = "SA";
}

/// <summary>
/// طلب الاسترجاع
/// </summary>
public class NoonRefundRequest
{
	[JsonPropertyName("apiOperation")]
	public string ApiOperation { get; set; } = "REFUND";

	[JsonPropertyName("order")]
	public NoonRefundOrderInfo Order { get; set; } = new();
}

/// <summary>
/// معلومات طلب الاسترجاع
/// </summary>
public class NoonRefundOrderInfo
{
	[JsonPropertyName("id")]
	public required string Id { get; set; }

	[JsonPropertyName("amount")]
	public decimal? Amount { get; set; }
}

/// <summary>
/// طلب الإلغاء
/// </summary>
public class NoonCancelRequest
{
	[JsonPropertyName("apiOperation")]
	public string ApiOperation { get; set; } = "REVERSE";
}

/// <summary>
/// طلب تأكيد الدفع
/// </summary>
public class NoonCaptureRequest
{
	[JsonPropertyName("apiOperation")]
	public string ApiOperation { get; set; } = "CAPTURE";

	[JsonPropertyName("order")]
	public NoonCaptureOrderInfo Order { get; set; } = new();
}

/// <summary>
/// معلومات طلب التأكيد
/// </summary>
public class NoonCaptureOrderInfo
{
	[JsonPropertyName("amount")]
	public decimal? Amount { get; set; }
}

#endregion

#region Response Models

/// <summary>
/// استجابة API نون
/// </summary>
public class NoonApiResponse
{
	[JsonPropertyName("resultCode")]
	public int ResultCode { get; set; }

	[JsonPropertyName("message")]
	public string? Message { get; set; }

	[JsonPropertyName("classificationCode")]
	public string? ClassificationCode { get; set; }

	[JsonPropertyName("result")]
	public NoonOrderResult? Result { get; set; }
}

/// <summary>
/// نتيجة الطلب
/// </summary>
public class NoonOrderResult
{
	[JsonPropertyName("order")]
	public NoonOrderResponse? Order { get; set; }

	[JsonPropertyName("checkoutData")]
	public NoonCheckoutData? CheckoutData { get; set; }
}

/// <summary>
/// تفاصيل الطلب في الاستجابة
/// </summary>
public class NoonOrderResponse
{
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	[JsonPropertyName("reference")]
	public string? Reference { get; set; }

	[JsonPropertyName("status")]
	public string? Status { get; set; }

	[JsonPropertyName("amount")]
	public decimal Amount { get; set; }

	[JsonPropertyName("currency")]
	public string? Currency { get; set; }

	[JsonPropertyName("channel")]
	public string? Channel { get; set; }

	[JsonPropertyName("category")]
	public string? Category { get; set; }

	[JsonPropertyName("creationTime")]
	public DateTime? CreationTime { get; set; }

	[JsonPropertyName("totalCapturedAmount")]
	public decimal? TotalCapturedAmount { get; set; }

	[JsonPropertyName("totalRefundedAmount")]
	public decimal? TotalRefundedAmount { get; set; }

	[JsonPropertyName("totalAuthorizedAmount")]
	public decimal? TotalAuthorizedAmount { get; set; }
}

/// <summary>
/// بيانات صفحة الدفع
/// </summary>
public class NoonCheckoutData
{
	[JsonPropertyName("postUrl")]
	public string? PostUrl { get; set; }
}

/// <summary>
/// حالات طلب نون
/// </summary>
public static class NoonOrderStatus
{
	public const string Initiated = "INITIATED";
	public const string Authorized = "AUTHORIZED";
	public const string Captured = "CAPTURED";
	public const string PaymentComplete = "PAYMENT_COMPLETE";
	public const string Failed = "FAILED";
	public const string Cancelled = "CANCELLED";
	public const string Reversed = "REVERSED";
	public const string Refunded = "REFUNDED";
	public const string PartiallyRefunded = "PARTIALLY_REFUNDED";
	public const string Expired = "EXPIRED";
}

/// <summary>
/// أكواد النتائج
/// </summary>
public static class NoonResultCode
{
	public const int Success = 0;
	public const int Pending = 100;
	public const int AuthenticationError = 101;
	public const int ValidationError = 400;
	public const int NotFound = 404;
	public const int InternalError = 500;
}

#endregion

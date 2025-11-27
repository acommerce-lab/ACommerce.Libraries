using ACommerce.Client.Core.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACommerce.Client.Payments;

public sealed class PaymentsClient
{
	private readonly DynamicHttpClient _httpClient;
	private const string ServiceName = "Payments"; // أو "Marketplace"

	public PaymentsClient(DynamicHttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// إنشاء دفعة جديدة
	/// </summary>
	public async Task<PaymentResponse?> CreatePaymentAsync(
		CreatePaymentRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<CreatePaymentRequest, PaymentResponse>(
			ServiceName,
			"/api/payments",
			request,
			cancellationToken);
	}

	/// <summary>
	/// الحصول على حالة الدفعة
	/// </summary>
	public async Task<PaymentResponse?> GetPaymentStatusAsync(
		string paymentId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<PaymentResponse>(
			ServiceName,
			$"/api/payments/{paymentId}",
			cancellationToken);
	}

	/// <summary>
	/// إلغاء دفعة
	/// </summary>
	public async Task<PaymentResponse?> CancelPaymentAsync(
		string paymentId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<object, PaymentResponse>(
			ServiceName,
			$"/api/payments/{paymentId}/cancel",
			new { },
			cancellationToken);
	}

	/// <summary>
	/// استرجاع مبلغ
	/// </summary>
	public async Task<RefundResponse?> RefundPaymentAsync(
		string paymentId,
		RefundRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<RefundRequest, RefundResponse>(
			ServiceName,
			$"/api/payments/{paymentId}/refund",
			request,
			cancellationToken);
	}
}

public sealed class CreatePaymentRequest
{
	public Guid OrderId { get; set; }
	public decimal Amount { get; set; }
	public string Currency { get; set; } = "SAR";
	public string PaymentMethod { get; set; } = string.Empty; // "CreditCard", "PayPal", "Mada", etc.
	[NotMapped] public Dictionary<string, string> Metadata { get; set; } = new();
}

public sealed class PaymentResponse
{
	public string PaymentId { get; set; } = string.Empty;
	public Guid OrderId { get; set; }
	public decimal Amount { get; set; }
	public string Currency { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty; // "Pending", "Completed", "Failed", "Cancelled"
	public string PaymentMethod { get; set; } = string.Empty;
	public string? PaymentUrl { get; set; } // للتوجيه لصفحة الدفع
	public DateTime CreatedAt { get; set; }
}

public sealed class RefundRequest
{
	public decimal Amount { get; set; }
	public string Reason { get; set; } = string.Empty;
}

public sealed class RefundResponse
{
	public string RefundId { get; set; } = string.Empty;
	public string PaymentId { get; set; } = string.Empty;
	public decimal Amount { get; set; }
	public string Status { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
}

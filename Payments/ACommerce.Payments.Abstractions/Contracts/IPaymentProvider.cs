using ACommerce.Payments.Abstractions.Models;

namespace ACommerce.Payments.Abstractions.Contracts;

/// <summary>
/// واجهة مزود الدفع
/// </summary>
public interface IPaymentProvider
{
	/// <summary>
	/// اسم المزود
	/// </summary>
	string ProviderName { get; }

	/// <summary>
	/// إنشاء عملية دفع
	/// </summary>
	Task<PaymentResult> CreatePaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// التحقق من حالة الدفع
	/// </summary>
	Task<PaymentResult> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default);

	/// <summary>
	/// استرجاع مبلغ
	/// </summary>
	Task<RefundResult> RefundAsync(RefundRequest request, CancellationToken cancellationToken = default);

	/// <summary>
	/// إلغاء عملية دفع
	/// </summary>
	Task<bool> CancelPaymentAsync(string transactionId, CancellationToken cancellationToken = default);

	/// <summary>
	/// التحقق من webhook
	/// </summary>
	Task<bool> ValidateWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default);
}

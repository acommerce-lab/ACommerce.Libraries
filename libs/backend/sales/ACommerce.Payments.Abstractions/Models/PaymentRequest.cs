using ACommerce.Payments.Abstractions.Enums;

namespace ACommerce.Payments.Abstractions.Models;

/// <summary>
/// طلب دفع
/// </summary>
public record PaymentRequest
{
	public required decimal Amount { get; init; }
	public required string Currency { get; init; }
	public required string OrderId { get; init; }
	public required string CustomerId { get; init; }
	public PaymentMethod? Method { get; init; }
	public string? CallbackUrl { get; init; }
	public string? Description { get; init; }
	public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// نتيجة الدفع
/// </summary>
public record PaymentResult
{
	public required bool Success { get; init; }
	public required string TransactionId { get; init; }
	public required PaymentStatus Status { get; init; }
	public string? PaymentUrl { get; init; }
	public string? ErrorMessage { get; init; }
	public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// طلب استرجاع مبلغ
/// </summary>
public record RefundRequest
{
	public required string TransactionId { get; init; }
	public required decimal Amount { get; init; }
	public string? Reason { get; init; }
}

/// <summary>
/// نتيجة الاسترجاع
/// </summary>
public record RefundResult
{
	public required bool Success { get; init; }
	public required string RefundId { get; init; }
	public string? ErrorMessage { get; init; }
}

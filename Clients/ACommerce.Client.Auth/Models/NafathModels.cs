namespace ACommerce.Client.Auth.Models;

/// <summary>
/// طلب بدء مصادقة نفاذ
/// </summary>
public sealed class NafathAuthRequest
{
	/// <summary>
	/// رقم الهوية الوطنية
	/// </summary>
	public string NationalId { get; set; } = string.Empty;

	/// <summary>
	/// رابط إعادة التوجيه بعد المصادقة
	/// </summary>
	public string? RedirectUrl { get; set; }
}

/// <summary>
/// استجابة بدء مصادقة نفاذ
/// </summary>
public sealed class NafathAuthResponse
{
	public bool Success { get; set; }
	public string? SessionId { get; set; }
	public string? TransactionId { get; set; }
	public string? RandomNumber { get; set; }
	public int? ExpiresInSeconds { get; set; }
	public string? Message { get; set; }
}

/// <summary>
/// استجابة أرقام الجوال من نفاذ
/// </summary>
public sealed class NafathPhoneNumbersResponse
{
	public List<string> PhoneNumbers { get; set; } = new();
	public string? SessionId { get; set; }
}

/// <summary>
/// طلب اختيار رقم الجوال من نفاذ
/// </summary>
public sealed class SelectNafathNumberRequest
{
	public string SessionId { get; set; } = string.Empty;
	public string PhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// طلب إكمال مصادقة نفاذ
/// </summary>
public sealed class CompleteNafathAuthRequest
{
	public string SessionId { get; set; } = string.Empty;
	public string TransactionId { get; set; } = string.Empty;
	public string? Code { get; set; }
	public string? State { get; set; }
}

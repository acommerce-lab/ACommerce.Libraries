namespace ACommerce.Client.Auth.Models;

/// <summary>
/// طلب رمز OTP عبر الهاتف
/// </summary>
public sealed class RequestPhoneOtpRequest
{
	public string PhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// طلب رمز OTP عبر البريد الإلكتروني
/// </summary>
public sealed class RequestEmailOtpRequest
{
	public string Email { get; set; } = string.Empty;
}

/// <summary>
/// التحقق من رمز OTP
/// </summary>
public sealed class VerifyOtpRequest
{
	public string PhoneOrEmail { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
}

/// <summary>
/// استجابة طلب OTP
/// </summary>
public sealed class OtpResponse
{
	public bool Success { get; set; }
	public string? Message { get; set; }
	public int? ExpiresInSeconds { get; set; }
	public string? SessionId { get; set; }
}

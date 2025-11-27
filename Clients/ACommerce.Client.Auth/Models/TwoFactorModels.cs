namespace ACommerce.Client.Auth.Models;

/// <summary>
/// طلب تفعيل/إلغاء المصادقة الثنائية
/// </summary>
public sealed class ToggleTwoFactorRequest
{
	public bool Enable { get; set; }
}

/// <summary>
/// استجابة تفعيل المصادقة الثنائية
/// </summary>
public sealed class TwoFactorResponse
{
	public bool IsEnabled { get; set; }
	public string? QrCodeUrl { get; set; }
	public string? SecretKey { get; set; }
	public string[]? RecoveryCodes { get; set; }
}

/// <summary>
/// طلب التحقق من كود المصادقة الثنائية
/// </summary>
public sealed class VerifyTwoFactorRequest
{
	public string Code { get; set; } = string.Empty;
}

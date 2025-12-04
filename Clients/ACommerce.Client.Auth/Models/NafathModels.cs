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
    public string? TransactionId { get; set; }
    /// <summary>
    /// الرقم العشوائي الذي يجب اختياره في تطبيق نفاذ
    /// </summary>
    public string? RandomNumber { get; set; }
    /// <summary>
    /// رابط صفحة المصادقة (للتوجيه في المتصفح)
    /// </summary>
    public string? AuthUrl { get; set; }
    public int ExpiresInSeconds { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// استجابة حالة مصادقة نفاذ
/// </summary>
public sealed class NafathStatusResponse
{
    public string? TransactionId { get; set; }
    /// <summary>
    /// pending, completed, expired, rejected
    /// </summary>
    public string Status { get; set; } = "pending";
    public string? Message { get; set; }
}

/// <summary>
/// طلب إكمال مصادقة نفاذ
/// </summary>
public sealed class CompleteNafathRequest
{
    public string TransactionId { get; set; } = string.Empty;
}

/// <summary>
/// استجابة تسجيل الدخول
/// </summary>
public sealed class LoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? ProfileId { get; set; }
    public string? FullName { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Message { get; set; }
    /// <summary>
    /// يتطلب التحقق بخطوتين (OTP)
    /// </summary>
    public bool RequiresTwoFactor { get; set; }
    /// <summary>
    /// طريقة التحقق بخطوتين (Phone, Email)
    /// </summary>
    public string? TwoFactorMethod { get; set; }
    /// <summary>
    /// معرف الجلسة للتحقق بخطوتين
    /// </summary>
    public string? SessionId { get; set; }
}

/// <summary>
/// معلومات البروفايل
/// </summary>
public sealed class ProfileResponse
{
    public string? Id { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsVerified { get; set; }
}

#region Additional Auth Models

/// <summary>
/// طلب تسجيل الدخول بالبريد الإلكتروني
/// </summary>
public sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// طلب التسجيل
/// </summary>
public sealed class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// طلب إرسال كود OTP للهاتف
/// </summary>
public sealed class RequestPhoneOtpRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// طلب إرسال كود OTP للبريد
/// </summary>
public sealed class RequestEmailOtpRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// استجابة طلب OTP
/// </summary>
public sealed class OtpResponse
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public int ExpiresInSeconds { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// طلب التحقق من OTP
/// </summary>
public sealed class VerifyTwoFactorRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    /// <summary>
    /// رقم الهاتف أو البريد الإلكتروني للتحقق
    /// </summary>
    public string PhoneOrEmail { get; set; } = string.Empty;
}

/// <summary>
/// طلب اختيار رقم نفاذ
/// </summary>
public sealed class SelectNafathNumberRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    /// <summary>
    /// معرف الجلسة
    /// </summary>
    public string? SessionId { get; set; }
}

/// <summary>
/// طلب إكمال مصادقة نفاذ بعد الـ callback
/// </summary>
public sealed class CompleteNafathAuthRequest
{
    public string Code { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}

/// <summary>
/// استجابة أرقام الهواتف المتاحة من نفاذ
/// </summary>
public sealed class NafathPhoneNumbersResponse
{
    public bool Success { get; set; }
    public List<string> PhoneNumbers { get; set; } = new();
    public string? TransactionId { get; set; }
    public string? Message { get; set; }
}

#endregion

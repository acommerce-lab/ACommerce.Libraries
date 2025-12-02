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

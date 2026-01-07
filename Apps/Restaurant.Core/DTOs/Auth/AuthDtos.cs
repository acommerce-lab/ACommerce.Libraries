namespace Restaurant.Core.DTOs.Auth;

/// <summary>
/// طلب إرسال رمز OTP للهاتف
/// </summary>
public class SendOtpRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string? CountryCode { get; set; } = "+966";
}

/// <summary>
/// نتيجة إرسال OTP
/// </summary>
public class SendOtpResponse
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? Message { get; set; }
    public int ExpiresInSeconds { get; set; } = 300;
}

/// <summary>
/// طلب التحقق من OTP
/// </summary>
public class VerifyOtpRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// نتيجة تسجيل الدخول
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserInfo? User { get; set; }
    public string? Error { get; set; }
    public bool IsNewUser { get; set; }
}

/// <summary>
/// معلومات المستخدم
/// </summary>
public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? ImageUrl { get; set; }
    public string UserType { get; set; } = string.Empty; // Customer, Vendor, Driver
    public Guid? RestaurantId { get; set; } // للمطعم والسائق
}

/// <summary>
/// طلب تحديث معلومات المستخدم
/// </summary>
public class UpdateProfileRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>
/// طلب تسجيل مستخدم جديد (بعد التحقق من OTP)
/// </summary>
public class RegisterRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}

/// <summary>
/// طلب تسجيل دخول المطعم
/// </summary>
public class VendorLoginRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
    public Guid? RestaurantId { get; set; }
}

/// <summary>
/// طلب تسجيل دخول السائق
/// </summary>
public class DriverLoginRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}

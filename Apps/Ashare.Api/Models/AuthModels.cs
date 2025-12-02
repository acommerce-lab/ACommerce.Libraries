namespace Ashare.Api.Models;

/// <summary>
/// طلب تسجيل مستخدم جديد
/// </summary>
public sealed class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ConfirmPassword { get; set; }
    public string Role { get; set; } = "Customer";
    public bool AcceptTerms { get; set; }
}

/// <summary>
/// طلب تسجيل الدخول
/// </summary>
public sealed class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// استجابة تسجيل الدخول/التسجيل
/// </summary>
public sealed class LoginResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool RequiresTwoFactor { get; set; }
    public bool RequiresNumberSelection { get; set; }
    public string? TwoFactorMethod { get; set; }
    public string? SessionId { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// معلومات المستخدم الحالي
/// </summary>
public sealed class UserInfoResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

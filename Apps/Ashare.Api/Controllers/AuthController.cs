using Microsoft.AspNetCore.Mvc;
using ACommerce.Authentication.Nafath.Models;

namespace Ashare.Api.Controllers;

/// <summary>
/// المصادقة والتحقق من الهوية
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// بدء تسجيل الدخول عبر نفاذ
    /// </summary>
    [HttpPost("nafath/initiate")]
    public async Task<ActionResult<NafathInitiateResponse>> InitiateNafath([FromBody] NafathInitiateRequest request)
    {
        _logger.LogDebug("Initiating Nafath login for: {NationalId}", request.NationalId);
        // Return random number for user to select in Nafath app
        var random = new Random();
        return Ok(new NafathInitiateResponse
        {
            TransactionId = Guid.NewGuid().ToString(),
            RandomNumber = random.Next(10, 99).ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(2)
        });
    }

    /// <summary>
    /// التحقق من حالة نفاذ
    /// </summary>
    [HttpPost("nafath/verify")]
    public async Task<ActionResult<NafathVerifyResponse>> VerifyNafath([FromBody] NafathVerifyRequest request)
    {
        _logger.LogDebug("Verifying Nafath transaction: {TransactionId}", request.TransactionId);
        return Ok(new NafathVerifyResponse
        {
            IsVerified = true,
            AccessToken = "jwt_token_here",
            RefreshToken = "refresh_token_here",
            ExpiresIn = 3600
        });
    }

    /// <summary>
    /// تسجيل الدخول بالبريد وكلمة المرور
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogDebug("Login attempt for: {Email}", request.Email);
        return Ok(new LoginResponse
        {
            AccessToken = "jwt_token_here",
            RefreshToken = "refresh_token_here",
            ExpiresIn = 3600
        });
    }

    /// <summary>
    /// تسجيل مستخدم جديد
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
    {
        _logger.LogDebug("Registering new user: {Email}", request.Email);
        return Ok(new RegisterResponse
        {
            UserId = Guid.NewGuid(),
            Message = "تم التسجيل بنجاح. يرجى التحقق من بريدك الإلكتروني."
        });
    }

    /// <summary>
    /// تحديث رمز الوصول
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        _logger.LogDebug("Refreshing token");
        return Ok(new LoginResponse
        {
            AccessToken = "new_jwt_token_here",
            RefreshToken = "new_refresh_token_here",
            ExpiresIn = 3600
        });
    }

    /// <summary>
    /// تسجيل الخروج
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        _logger.LogDebug("User logging out");
        return Ok(new { message = "تم تسجيل الخروج بنجاح" });
    }

    /// <summary>
    /// طلب استعادة كلمة المرور
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        _logger.LogDebug("Forgot password request for: {Email}", request.Email);
        return Ok(new { message = "تم إرسال رابط استعادة كلمة المرور إلى بريدك الإلكتروني" });
    }

    /// <summary>
    /// إعادة تعيين كلمة المرور
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        _logger.LogDebug("Reset password request");
        return Ok(new { message = "تم تغيير كلمة المرور بنجاح" });
    }
}

// Request/Response Models
public class NafathInitiateRequest
{
    public string NationalId { get; set; } = default!;
}

public class NafathInitiateResponse
{
    public string TransactionId { get; set; } = default!;
    public string RandomNumber { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
}

public class NafathVerifyRequest
{
    public string TransactionId { get; set; } = default!;
}

public class NafathVerifyResponse
{
    public bool IsVerified { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public string? ErrorMessage { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public int ExpiresIn { get; set; }
}

public class RegisterRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? Phone { get; set; }
    public string? NationalId { get; set; }
}

public class RegisterResponse
{
    public Guid UserId { get; set; }
    public string Message { get; set; } = default!;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = default!;
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = default!;
}

public class ResetPasswordRequest
{
    public string Token { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}

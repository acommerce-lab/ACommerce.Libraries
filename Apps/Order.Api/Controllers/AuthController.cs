using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ACommerce.Authentication.Abstractions;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Profiles.Entities;
using ACommerce.Profiles.Enums;

namespace Order.Api.Controllers;

/// <summary>
/// تسجيل الدخول برقم الهاتف (محاكاة SMS)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationProvider _authProvider;
    private readonly IBaseAsyncRepository<Profile> _profileRepository;
    private readonly ILogger<AuthController> _logger;

    // قائمة أكواد التحقق المؤقتة (في الإنتاج ستكون Redis أو Database)
    private static readonly Dictionary<string, (string Code, DateTime Expiry, Guid? ProfileId)> _verificationCodes = new();

    public AuthController(
        IAuthenticationProvider authProvider,
        IBaseAsyncRepository<Profile> profileRepository,
        ILogger<AuthController> logger)
    {
        _authProvider = authProvider;
        _profileRepository = profileRepository;
        _logger = logger;
    }

    /// <summary>
    /// إرسال كود التحقق للهاتف
    /// </summary>
    [HttpPost("send-code")]
    public async Task<IActionResult> SendVerificationCode([FromBody] SendCodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            return BadRequest(new { Message = "رقم الهاتف مطلوب" });

        // تنظيف رقم الهاتف
        var phone = request.PhoneNumber.Trim().Replace(" ", "");
        if (!phone.StartsWith("+"))
            phone = "+966" + phone.TrimStart('0');

        // توليد كود عشوائي (4 أرقام)
        var code = new Random().Next(1000, 9999).ToString();

        // البحث عن المستخدم
        var profiles = await _profileRepository.GetAllWithPredicateAsync(p => p.PhoneNumber == phone);
        var profile = profiles.FirstOrDefault();

        // حفظ الكود
        _verificationCodes[phone] = (code, DateTime.UtcNow.AddMinutes(5), profile?.Id);

        // في الإنتاج: إرسال SMS حقيقي
        // حالياً: نطبع الكود في اللوق للاختبار
        _logger.LogInformation("📱 Verification code for {Phone}: {Code}", phone, code);

        return Ok(new
        {
            Message = "تم إرسال كود التحقق",
            Phone = phone,
            ExpiresInSeconds = 300,
            // في التطوير فقط - نرسل الكود مباشرة
            DebugCode = code
        });
    }

    /// <summary>
    /// التحقق من الكود وتسجيل الدخول
    /// </summary>
    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(new { Message = "رقم الهاتف والكود مطلوبان" });

        var phone = request.PhoneNumber.Trim().Replace(" ", "");
        if (!phone.StartsWith("+"))
            phone = "+966" + phone.TrimStart('0');

        // التحقق من الكود
        if (!_verificationCodes.TryGetValue(phone, out var stored))
            return BadRequest(new { Message = "لم يتم إرسال كود لهذا الرقم" });

        if (DateTime.UtcNow > stored.Expiry)
        {
            _verificationCodes.Remove(phone);
            return BadRequest(new { Message = "انتهت صلاحية الكود" });
        }

        if (stored.Code != request.Code)
            return BadRequest(new { Message = "الكود غير صحيح" });

        // إزالة الكود بعد الاستخدام
        _verificationCodes.Remove(phone);

        // البحث أو إنشاء Profile
        Profile? profile;
        bool isNewUser = false;

        if (stored.ProfileId.HasValue)
        {
            profile = await _profileRepository.GetByIdAsync(stored.ProfileId.Value);
        }
        else
        {
            // إنشاء مستخدم جديد
            var profileId = Guid.NewGuid();
            profile = new Profile
            {
                Id = profileId,
                UserId = profileId.ToString(),
                PhoneNumber = phone,
                FullName = "مستخدم جديد",
                Type = ProfileType.Customer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _profileRepository.AddAsync(profile);
            isNewUser = true;
            _logger.LogInformation("Created new profile for {Phone}", phone);
        }

        if (profile == null)
            return BadRequest(new { Message = "خطأ في تحميل الملف الشخصي" });

        // إنشاء JWT Token باستخدام AuthenticationProvider
        var authResult = await _authProvider.AuthenticateAsync(new AuthenticationRequest
        {
            Identifier = profile.Id.ToString(),
            Claims = new Dictionary<string, string>
            {
                [ClaimTypes.NameIdentifier] = profile.Id.ToString(),
                [ClaimTypes.Name] = profile.FullName ?? "",
                [ClaimTypes.MobilePhone] = phone,
                ["profile_type"] = "Customer"
            }
        });

        if (!authResult.Success)
        {
            return BadRequest(new { Message = "فشل إنشاء جلسة الدخول" });
        }

        return Ok(new LoginResponse
        {
            Success = true,
            Token = authResult.AccessToken ?? "",
            ProfileId = profile.Id.ToString(),
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber,
            Avatar = profile.Avatar,
            ExpiresAt = authResult.ExpiresAt?.DateTime ?? DateTime.UtcNow.AddDays(7),
            Message = isNewUser ? "مرحباً بك! أكمل بياناتك" : "تم تسجيل الدخول بنجاح",
            IsNewUser = isNewUser
        });
    }

    /// <summary>
    /// الحصول على معلومات المستخدم الحالي
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var profileId))
            return Unauthorized(new { Message = "غير مصرح" });

        var profile = await _profileRepository.GetByIdAsync(profileId);

        if (profile == null)
            return NotFound(new { Message = "المستخدم غير موجود" });

        return Ok(new ProfileResponse
        {
            Id = profile.Id.ToString(),
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber,
            Email = profile.Email,
            Avatar = profile.Avatar,
            IsVerified = profile.IsVerified
        });
    }

    /// <summary>
    /// تحديث الملف الشخصي
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var profileId))
            return Unauthorized(new { Message = "غير مصرح" });

        var profile = await _profileRepository.GetByIdAsync(profileId);

        if (profile == null)
            return NotFound(new { Message = "المستخدم غير موجود" });

        if (!string.IsNullOrWhiteSpace(request.FullName))
            profile.FullName = request.FullName;

        if (!string.IsNullOrWhiteSpace(request.Email))
            profile.Email = request.Email;

        profile.UpdatedAt = DateTime.UtcNow;
        await _profileRepository.UpdateAsync(profile);

        return Ok(new ProfileResponse
        {
            Id = profile.Id.ToString(),
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber,
            Email = profile.Email,
            Avatar = profile.Avatar,
            IsVerified = profile.IsVerified
        });
    }
}

#region DTOs

public record SendCodeRequest(string PhoneNumber);
public record VerifyCodeRequest(string PhoneNumber, string Code);
public record UpdateProfileRequest(string? FullName, string? Email);

public class LoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? ProfileId { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Avatar { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Message { get; set; }
    public bool IsNewUser { get; set; }
}

public class ProfileResponse
{
    public string? Id { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Avatar { get; set; }
    public bool IsVerified { get; set; }
}

#endregion

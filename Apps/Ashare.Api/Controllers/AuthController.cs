using System.Security.Claims;
using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.AspNetCore.Controllers;
using ACommerce.Profiles.Entities;
using ACommerce.Profiles.Enums;
using ACommerce.SharedKernel.Abstractions.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ashare.Api.Controllers;

/// <summary>
/// Controller للمصادقة عبر نفاذ فقط
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : AuthenticationControllerBase
{
    private readonly IBaseAsyncRepository<Profile> _profileRepository;
    private readonly DbContext _dbContext;

    public AuthController(
        IAuthenticationProvider authProvider,
        ITwoFactorAuthenticationProvider twoFactorProvider,
        IBaseAsyncRepository<Profile> profileRepository,
        DbContext dbContext,
        ILogger<AuthController> logger)
        : base(authProvider, twoFactorProvider, logger)
    {
        _profileRepository = profileRepository;
        _dbContext = dbContext;
    }

    #region Nafath Authentication

    /// <summary>
    /// بدء مصادقة نفاذ - يرسل رقم الهوية ويستلم الكود العشوائي
    /// POST /api/auth/nafath/initiate
    /// </summary>
    [HttpPost("nafath/initiate")]
    public async Task<ActionResult<NafathAuthResponse>> InitiateNafathAuth([FromBody] NafathAuthRequest request)
    {
        try
        {
            Logger.LogInformation("بدء مصادقة نفاذ للهوية: {NationalId}", request.NationalId);

            var result = await TwoFactorProvider.InitiateAsync(
                new TwoFactorInitiationRequest
                {
                    Identifier = request.NationalId
                });

            if (!result.Success)
            {
                return BadRequest(new NafathAuthResponse
                {
                    Success = false,
                    Message = result.Error?.Message ?? "فشل بدء مصادقة نفاذ"
                });
            }

            return Ok(new NafathAuthResponse
            {
                Success = true,
                TransactionId = result.TransactionId,
                RandomNumber = result.Data?.GetValueOrDefault("verificationCode") ?? result.VerificationCode,
                ExpiresInSeconds = (int?)result.ExpiresIn?.TotalSeconds ?? 120,
                Message = "افتح تطبيق نفاذ واختر الرقم المعروض"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "خطأ في بدء مصادقة نفاذ");
            return BadRequest(new NafathAuthResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء بدء المصادقة"
            });
        }
    }

    /// <summary>
    /// التحقق من حالة المصادقة (polling)
    /// GET /api/auth/nafath/status?transactionId=xxx
    /// </summary>
    [HttpGet("nafath/status")]
    public async Task<ActionResult<NafathStatusResponse>> CheckNafathStatus([FromQuery] string transactionId)
    {
        try
        {
            // في التنفيذ الحقيقي، نتحقق من حالة الجلسة في نفاذ
            // حالياً نرجع "pending" حتى يأتي webhook
            return Ok(new NafathStatusResponse
            {
                TransactionId = transactionId,
                Status = "pending",
                Message = "في انتظار اختيار الرقم في تطبيق نفاذ"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "خطأ في التحقق من حالة نفاذ");
            return BadRequest(new NafathStatusResponse
            {
                TransactionId = transactionId,
                Status = "error",
                Message = "حدث خطأ"
            });
        }
    }

    /// <summary>
    /// إكمال مصادقة نفاذ بعد نجاح التحقق
    /// POST /api/auth/nafath/complete
    /// </summary>
    [HttpPost("nafath/complete")]
    public async Task<ActionResult<LoginResponse>> CompleteNafathAuth([FromBody] CompleteNafathRequest request)
    {
        try
        {
            Logger.LogInformation("إكمال مصادقة نفاذ: {TransactionId}", request.TransactionId);

            var result = await TwoFactorProvider.VerifyAsync(
                new TwoFactorVerificationRequest
                {
                    TransactionId = request.TransactionId
                });

            if (!result.Success)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = result.Error?.Message ?? "فشل التحقق"
                });
            }

            // البحث عن البروفايل أو إنشاء جديد
            var nationalId = result.Data?["national_id"] ?? request.TransactionId;
            var (profile, isNewUser) = await GetOrCreateProfile(nationalId, result.Data);

            // إنشاء التوكن
            var authResult = await AuthProvider.AuthenticateAsync(new AuthenticationRequest
            {
                Identifier = profile.Id.ToString(),
                Claims = new Dictionary<string, string>
                {
                    [ClaimTypes.NameIdentifier] = profile.Id.ToString(),
                    [ClaimTypes.Name] = profile.FullName ?? "",
                    ["national_id"] = nationalId
                }
            });

            if (!authResult.Success)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "فشل إنشاء الجلسة"
                });
            }

            return Ok(new LoginResponse
            {
                Success = true,
                Token = authResult.AccessToken ?? "",
                ProfileId = profile.Id.ToString(),
                FullName = profile.FullName,
                ExpiresAt = authResult.ExpiresAt?.DateTime ?? DateTime.UtcNow.AddDays(7),
                Message = isNewUser ? "مرحباً بك! أكمل بياناتك" : "تم تسجيل الدخول بنجاح",
                IsNewUser = isNewUser
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "خطأ في إكمال مصادقة نفاذ");
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "حدث خطأ"
            });
        }
    }

    /// <summary>
    /// محاكاة webhook نفاذ (للاختبار فقط)
    /// POST /api/auth/nafath/test-webhook
    /// </summary>
    //[HttpPost("nafath/test-webhook")]
    //public async Task<IActionResult> SimulateNafathWebhook([FromBody] TestWebhookRequest request)
    //{
    //    try
    //    {
    //        // التحقق من أننا في وضع الاختبار
    //        var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
    //        var mode = configuration["Authentication:TwoFactor:Nafath:Mode"]?.ToLower();

    //        if (mode != "test")
    //        {
    //            return BadRequest(new { success = false, message = "هذا الـ endpoint متاح فقط في وضع الاختبار" });
    //        }

    //        Logger.LogInformation("[Test Webhook] Simulating Nafath callback for {TransactionId}", request.TransactionId);

    //        // الحصول على provider ومعالجة الـ webhook
    //        if (TwoFactorProvider is ACommerce.Authentication.TwoFactor.Nafath.NafathAuthenticationProvider nafathProvider)
    //        {
    //            var webhookRequest = new ACommerce.Authentication.TwoFactor.Nafath.NafathWebhookRequest
    //            {
    //                TransactionId = request.TransactionId,
    //                NationalId = request.NationalId ?? "test_national_id",
    //                Status = request.Status ?? "COMPLETED"
    //            };

    //            var result = await nafathProvider.HandleWebhookAsync(webhookRequest);

    //            if (result)
    //            {
    //                Logger.LogInformation("[Test Webhook] Successfully simulated webhook for {TransactionId}", request.TransactionId);
    //                return Ok(new { success = true, message = "تم محاكاة التحقق بنجاح" });
    //            }
    //        }

    //        return BadRequest(new { success = false, message = "فشل محاكاة التحقق" });
    //    }
    //    catch (Exception ex)
    //    {
    //        Logger.LogError(ex, "[Test Webhook] Error simulating webhook");
    //        return BadRequest(new { success = false, message = ex.Message });
    //    }
    //}

    #endregion

    #region User Info

    /// <summary>
    /// الحصول على معلومات المستخدم الحالي
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ProfileResponse>> GetMe()
    {
        var profileId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(profileId) || !Guid.TryParse(profileId, out var id))
        {
            return Unauthorized();
        }

        var profile = await _profileRepository.GetByIdAsync(id);
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(new ProfileResponse
        {
            Id = profile.Id.ToString(),
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber,
            ProfilePictureUrl = profile.Avatar,
            IsVerified = profile.IsVerified
        });
    }

    /// <summary>
    /// تسجيل الخروج
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Logger.LogInformation("تسجيل خروج: {ProfileId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        return Ok(new { success = true, message = "تم تسجيل الخروج" });
    }

    #endregion

    #region Private Methods

    private async Task<(Profile Profile, bool IsNewUser)> GetOrCreateProfile(string nationalId, Dictionary<string, string>? nafathData)
    {
        // البحث عن بروفايل موجود بنفس الهوية
        var existingProfile = await _dbContext.Set<Profile>()
            .FirstOrDefaultAsync(p => p.NationalId == nationalId);

        if (existingProfile != null)
        {
            return (existingProfile, false);
        }

        // إنشاء بروفايل جديد
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            NationalId = nationalId,
            FullName = nafathData?.GetValueOrDefault("full_name") ?? "",
            PhoneNumber = nafathData?.GetValueOrDefault("phone_number") ?? "",
            Type = ProfileType.Customer,
            IsActive = true,
            IsVerified = true, // تم التحقق عبر نفاذ
            CreatedAt = DateTime.UtcNow
        };

        await _profileRepository.AddAsync(profile);
        await _dbContext.SaveChangesAsync();

        Logger.LogInformation("تم إنشاء بروفايل جديد: {ProfileId}", profile.Id);
        return (profile, true);
    }

    #endregion
}

#region Models

/// <summary>
/// طلب بدء مصادقة نفاذ
/// </summary>
public class NafathAuthRequest
{
    /// <summary>
    /// رقم الهوية الوطنية
    /// </summary>
    public string NationalId { get; set; } = string.Empty;
}

/// <summary>
/// استجابة بدء مصادقة نفاذ
/// </summary>
public class NafathAuthResponse
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
public class NafathStatusResponse
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
public class CompleteNafathRequest
{
    public string TransactionId { get; set; } = string.Empty;
}

/// <summary>
/// طلب محاكاة webhook (للاختبار فقط)
/// </summary>
public class TestWebhookRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// استجابة تسجيل الدخول
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? ProfileId { get; set; }
    public string? FullName { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Message { get; set; }

    /// <summary>
    /// هل هذا مستخدم جديد (يحتاج إكمال البروفايل)
    /// </summary>
    public bool IsNewUser { get; set; }
}

/// <summary>
/// معلومات البروفايل
/// </summary>
public class ProfileResponse
{
    public string? Id { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public bool IsVerified { get; set; }
}

#endregion

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ACommerce.Authentication.JWT;
using ACommerce.Authentication.Users.Abstractions;
using ACommerce.SharedKernel.Abstractions.Repositories;
using ACommerce.Profiles.Entities;

namespace Order.Api.Controllers;

/// <summary>
/// تسجيل الدخول برقم الهاتف (محاكاة SMS)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IJwtTokenService _tokenService;
    private readonly IUserProvider _userProvider;
    private readonly IRepositoryFactory _repositoryFactory;
    private readonly ILogger<AuthController> _logger;

    // قائمة أكواد التحقق المؤقتة (في الإنتاج ستكون Redis أو Database)
    private static readonly Dictionary<string, (string Code, DateTime Expiry, Guid? ProfileId)> _verificationCodes = new();

    public AuthController(
        IJwtTokenService tokenService,
        IUserProvider userProvider,
        IRepositoryFactory repositoryFactory,
        ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _userProvider = userProvider;
        _repositoryFactory = repositoryFactory;
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
        var profileRepo = _repositoryFactory.CreateRepository<Profile>();
        var profile = (await profileRepo.FindAsync(p => p.PhoneNumber == phone)).FirstOrDefault();

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
        var profileRepo = _repositoryFactory.CreateRepository<Profile>();
        Profile? profile;

        if (stored.ProfileId.HasValue)
        {
            profile = await profileRepo.GetByIdAsync(stored.ProfileId.Value);
        }
        else
        {
            // إنشاء مستخدم جديد
            profile = new Profile
            {
                Id = Guid.NewGuid(),
                PhoneNumber = phone,
                FirstName = "مستخدم",
                LastName = "جديد"
            };
            await profileRepo.AddAsync(profile);
            _logger.LogInformation("Created new profile for {Phone}", phone);
        }

        if (profile == null)
            return BadRequest(new { Message = "خطأ في تحميل الملف الشخصي" });

        // تسجيل المستخدم في UserProvider
        var user = new UserInfo(profile.Id.ToString(), profile.PhoneNumber ?? "", "Customer");
        await _userProvider.RegisterUserAsync(user);

        // إنشاء JWT Token
        var token = _tokenService.GenerateToken(profile.Id.ToString(), profile.PhoneNumber ?? "", "Customer");

        return Ok(new
        {
            Token = token,
            Profile = new
            {
                profile.Id,
                profile.FirstName,
                profile.LastName,
                profile.PhoneNumber,
                profile.Email,
                profile.AvatarUrl
            },
            Message = "تم تسجيل الدخول بنجاح"
        });
    }

    /// <summary>
    /// الحصول على معلومات المستخدم الحالي
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var profileId))
            return Unauthorized(new { Message = "غير مصرح" });

        var profileRepo = _repositoryFactory.CreateRepository<Profile>();
        var profile = await profileRepo.GetByIdAsync(profileId);

        if (profile == null)
            return NotFound(new { Message = "المستخدم غير موجود" });

        return Ok(new
        {
            profile.Id,
            profile.FirstName,
            profile.LastName,
            profile.PhoneNumber,
            profile.Email,
            profile.AvatarUrl
        });
    }

    /// <summary>
    /// تحديث الملف الشخصي
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var profileId))
            return Unauthorized(new { Message = "غير مصرح" });

        var profileRepo = _repositoryFactory.CreateRepository<Profile>();
        var profile = await profileRepo.GetByIdAsync(profileId);

        if (profile == null)
            return NotFound(new { Message = "المستخدم غير موجود" });

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            profile.FirstName = request.FirstName;

        if (!string.IsNullOrWhiteSpace(request.LastName))
            profile.LastName = request.LastName;

        if (!string.IsNullOrWhiteSpace(request.Email))
            profile.Email = request.Email;

        await profileRepo.UpdateAsync(profile);

        return Ok(new
        {
            profile.Id,
            profile.FirstName,
            profile.LastName,
            profile.PhoneNumber,
            profile.Email,
            profile.AvatarUrl,
            Message = "تم تحديث الملف الشخصي"
        });
    }
}

public record SendCodeRequest(string PhoneNumber);
public record VerifyCodeRequest(string PhoneNumber, string Code);
public record UpdateProfileRequest(string? FirstName, string? LastName, string? Email);

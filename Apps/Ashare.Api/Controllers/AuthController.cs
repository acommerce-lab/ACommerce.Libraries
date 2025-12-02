using System.Security.Claims;
using ACommerce.Authentication.Abstractions;
using ACommerce.Authentication.AspNetCore.Controllers;
using ACommerce.Authentication.Users.Abstractions;
using ACommerce.Authentication.Users.Abstractions.Models;
using ACommerce.Profiles.Entities;
using ACommerce.Profiles.Enums;
using ACommerce.SharedKernel.Abstractions.Repositories;
using Ashare.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ashare.Api.Controllers;

/// <summary>
/// Controller للمصادقة وإدارة الحسابات
/// يرث من AuthenticationControllerBase للحصول على endpoints المصادقة الثنائية
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : AuthenticationControllerBase
{
    private readonly IUserProvider _userProvider;
    private readonly IBaseAsyncRepository<Profile> _profileRepository;
    private readonly DbContext _dbContext;

    public AuthController(
        IAuthenticationProvider authProvider,
        ITwoFactorAuthenticationProvider twoFactorProvider,
        IUserProvider userProvider,
        IBaseAsyncRepository<Profile> profileRepository,
        DbContext dbContext,
        ILogger<AuthController> logger)
        : base(authProvider, twoFactorProvider, logger)
    {
        _userProvider = userProvider;
        _profileRepository = profileRepository;
        _dbContext = dbContext;
    }

    /// <summary>
    /// تسجيل مستخدم جديد
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // التحقق من البيانات
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "البريد الإلكتروني مطلوب"
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "كلمة المرور مطلوبة"
                });
            }

            if (request.Password != request.ConfirmPassword)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "كلمة المرور غير متطابقة"
                });
            }

            // التحقق من عدم وجود مستخدم بنفس البريد
            var existingUser = await _userProvider.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "البريد الإلكتروني مسجل مسبقاً"
                });
            }

            // إنشاء المستخدم
            var createResult = await _userProvider.CreateUserAsync(new CreateUserRequest
            {
                Username = request.Username.Length > 0 ? request.Username : request.Email,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Password = request.Password,
                TwoFactorEnabled = false,
                Roles = [request.Role],
                Metadata = new Dictionary<string, string>
                {
                    ["FullName"] = request.FullName,
                    ["AcceptTerms"] = request.AcceptTerms.ToString()
                }
            });

            if (!createResult.Success || createResult.User == null)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = createResult.Error?.Message ?? "فشل إنشاء الحساب"
                });
            }

            // إنشاء البروفايل
            await CreateProfileForUser(createResult.User, request.FullName, request.PhoneNumber);

            // إنشاء التوكن
            var authResult = await AuthProvider.AuthenticateAsync(new AuthenticationRequest
            {
                Identifier = createResult.User.UserId,
                Claims = new Dictionary<string, string>
                {
                    [ClaimTypes.NameIdentifier] = createResult.User.UserId,
                    [ClaimTypes.Name] = createResult.User.Username,
                    [ClaimTypes.Email] = createResult.User.Email,
                    [ClaimTypes.Role] = request.Role
                }
            });

            if (!authResult.Success)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "فشل إنشاء جلسة المصادقة"
                });
            }

            Logger.LogInformation("New user registered: {Email}", request.Email);

            return Ok(new LoginResponse
            {
                Success = true,
                Token = authResult.AccessToken ?? "",
                Username = createResult.User.Username,
                Email = createResult.User.Email,
                Role = request.Role,
                ExpiresAt = authResult.ExpiresAt?.DateTime ?? DateTime.UtcNow.AddHours(1),
                Message = "تم إنشاء الحساب بنجاح"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during registration for {Email}", request.Email);
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء التسجيل"
            });
        }
    }

    /// <summary>
    /// تسجيل الدخول
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // البحث عن المستخدم
            var identifier = !string.IsNullOrWhiteSpace(request.Email) ? request.Email : request.Username;

            var user = await _userProvider.GetUserByEmailAsync(identifier)
                ?? await _userProvider.GetUserByUsernameAsync(identifier);

            if (user == null)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "البريد الإلكتروني أو كلمة المرور غير صحيحة"
                });
            }

            // التحقق من كلمة المرور
            var passwordValid = await _userProvider.VerifyPasswordAsync(user.UserId, request.Password);
            if (!passwordValid)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "البريد الإلكتروني أو كلمة المرور غير صحيحة"
                });
            }

            // التحقق من حالة الحساب
            if (!user.IsActive)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "الحساب غير مفعل"
                });
            }

            if (user.IsLocked)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "الحساب موقوف مؤقتاً"
                });
            }

            // التحقق من المصادقة الثنائية
            if (user.TwoFactorEnabled)
            {
                return Ok(new LoginResponse
                {
                    Success = true,
                    RequiresTwoFactor = true,
                    TwoFactorMethod = TwoFactorProvider.ProviderName,
                    SessionId = Guid.NewGuid().ToString(),
                    Message = "يرجى إكمال المصادقة الثنائية"
                });
            }

            var role = user.Roles.FirstOrDefault() ?? "Customer";

            // إنشاء التوكن
            var authResult = await AuthProvider.AuthenticateAsync(new AuthenticationRequest
            {
                Identifier = user.UserId,
                Claims = new Dictionary<string, string>
                {
                    [ClaimTypes.NameIdentifier] = user.UserId,
                    [ClaimTypes.Name] = user.Username,
                    [ClaimTypes.Email] = user.Email,
                    [ClaimTypes.Role] = role
                }
            });

            if (!authResult.Success)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "فشل إنشاء جلسة المصادقة"
                });
            }

            Logger.LogInformation("User logged in: {Email}", user.Email);

            return Ok(new LoginResponse
            {
                Success = true,
                Token = authResult.AccessToken ?? "",
                Username = user.Username,
                Email = user.Email,
                Role = role,
                ExpiresAt = authResult.ExpiresAt?.DateTime ?? DateTime.UtcNow.AddHours(1),
                Message = "تم تسجيل الدخول بنجاح"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during login for {Identifier}", request.Email ?? request.Username);
            return Unauthorized(new LoginResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء تسجيل الدخول"
            });
        }
    }

    /// <summary>
    /// الحصول على معلومات المستخدم الحالي
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserInfoResponse>> GetMe()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userProvider.GetUserByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(new UserInfoResponse
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Roles.FirstOrDefault() ?? "Customer"
        });
    }

    /// <summary>
    /// تسجيل الخروج
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Logger.LogInformation("User logged out: {UserId}", userId);

        // JWT is stateless, but we can log the event
        // In production, you might want to add token to a blacklist

        return Ok(new { success = true, message = "تم تسجيل الخروج بنجاح" });
    }

    #region Nafath Authentication (Saudi Arabia)

    /// <summary>
    /// بدء مصادقة نفاذ
    /// POST /api/auth/nafath/initiate
    /// </summary>
    [HttpPost("nafath/initiate")]
    [ProducesResponseType(typeof(NafathAuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NafathAuthResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NafathAuthResponse>> InitiateNafathAuth([FromBody] NafathAuthRequest request)
    {
        try
        {
            Logger.LogInformation("Initiating Nafath authentication for NationalId: {NationalId}", request.NationalId);

            var result = await TwoFactorProvider.InitiateAsync(
                new TwoFactorInitiationRequest
                {
                    Identifier = request.NationalId,
                    Metadata = new Dictionary<string, string>
                    {
                        ["redirect_url"] = request.RedirectUrl ?? ""
                    }
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
                SessionId = result.TransactionId,
                TransactionId = result.TransactionId,
                RandomNumber = result.VerificationCode,
                ExpiresInSeconds = (int?)result.ExpiresIn?.TotalSeconds,
                Message = result.Message ?? "تم بدء المصادقة، يرجى فتح تطبيق نفاذ"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initiating Nafath authentication");
            return BadRequest(new NafathAuthResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء بدء المصادقة"
            });
        }
    }

    /// <summary>
    /// الحصول على أرقام الجوال المسجلة في نفاذ
    /// GET /api/auth/nafath/phone-numbers?sessionId=xxx
    /// </summary>
    [HttpGet("nafath/phone-numbers")]
    [ProducesResponseType(typeof(NafathPhoneNumbersResponse), StatusCodes.Status200OK)]
    public ActionResult<NafathPhoneNumbersResponse> GetNafathPhoneNumbers([FromQuery] string sessionId)
    {
        // في التنفيذ الحقيقي، يتم استرجاع أرقام الجوال من جلسة نفاذ
        // حالياً نرجع قائمة فارغة لأن نفاذ يعيد رقم واحد فقط عادة
        return Ok(new NafathPhoneNumbersResponse
        {
            SessionId = sessionId,
            PhoneNumbers = new List<string>()
        });
    }

    /// <summary>
    /// اختيار رقم الجوال من نفاذ
    /// POST /api/auth/nafath/select-number
    /// </summary>
    [HttpPost("nafath/select-number")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> SelectNafathNumber([FromBody] SelectNafathNumberRequest request)
    {
        try
        {
            Logger.LogInformation("Selecting Nafath phone number for session: {SessionId}", request.SessionId);

            // في التنفيذ الحقيقي، يتم التحقق من اختيار رقم الجوال
            // حالياً نقوم بتأكيد الجلسة فقط
            return Ok(new LoginResponse
            {
                Success = true,
                Message = "تم اختيار رقم الجوال، يرجى إكمال المصادقة في تطبيق نفاذ"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error selecting Nafath phone number");
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء اختيار رقم الجوال"
            });
        }
    }

    /// <summary>
    /// إكمال مصادقة نفاذ
    /// POST /api/auth/nafath/complete
    /// </summary>
    [HttpPost("nafath/complete")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> CompleteNafathAuth([FromBody] CompleteNafathAuthRequest request)
    {
        try
        {
            Logger.LogInformation("Completing Nafath authentication for session: {SessionId}", request.SessionId);

            var result = await TwoFactorProvider.VerifyAsync(
                new TwoFactorVerificationRequest
                {
                    TransactionId = request.TransactionId,
                    Code = request.Code,
                    Metadata = new Dictionary<string, string>
                    {
                        ["state"] = request.State ?? ""
                    }
                });

            if (!result.Success)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = result.Error?.Message ?? "فشل التحقق من المصادقة"
                });
            }

            // إنشاء التوكن بعد التحقق الناجح
            var authResult = await AuthProvider.AuthenticateAsync(new AuthenticationRequest
            {
                Identifier = result.UserId ?? request.TransactionId,
                Claims = result.UserClaims?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, string>(),
                Metadata = new Dictionary<string, object>
                {
                    ["two_factor_provider"] = TwoFactorProvider.ProviderName,
                    ["two_factor_verified"] = true
                }
            });

            if (!authResult.Success)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "فشل إنشاء جلسة المصادقة"
                });
            }

            Logger.LogInformation("Nafath authentication completed successfully for: {UserId}", result.UserId);

            return Ok(new LoginResponse
            {
                Success = true,
                Token = authResult.AccessToken ?? "",
                ExpiresAt = authResult.ExpiresAt?.DateTime ?? DateTime.UtcNow.AddHours(1),
                Message = "تم المصادقة بنجاح عبر نفاذ"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error completing Nafath authentication");
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء إكمال المصادقة"
            });
        }
    }

    #endregion

    #region Two-Factor Authentication

    /// <summary>
    /// تفعيل/إلغاء المصادقة الثنائية
    /// POST /api/auth/two-factor
    /// </summary>
    [HttpPost("two-factor")]
    [Authorize]
    [ProducesResponseType(typeof(TwoFactorResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TwoFactorResponse>> ToggleTwoFactor([FromBody] ToggleTwoFactorRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // في التنفيذ الحقيقي، يتم تحديث حالة المصادقة الثنائية للمستخدم
        Logger.LogInformation("Two-factor {Action} for user: {UserId}",
            request.Enable ? "enabled" : "disabled", userId);

        return Ok(new TwoFactorResponse
        {
            IsEnabled = request.Enable
        });
    }

    /// <summary>
    /// التحقق من كود المصادقة الثنائية
    /// POST /api/auth/two-factor/verify
    /// </summary>
    [HttpPost("two-factor/verify")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> VerifyTwoFactorCode([FromBody] VerifyTwoFactorRequest request)
    {
        try
        {
            Logger.LogInformation("Verifying two-factor code for session: {SessionId}", request.SessionId);

            var result = await TwoFactorProvider.VerifyAsync(
                new TwoFactorVerificationRequest
                {
                    TransactionId = request.SessionId ?? "",
                    Code = request.Code
                });

            if (!result.Success)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = result.Error?.Message ?? "رمز التحقق غير صحيح"
                });
            }

            // إنشاء التوكن
            var authResult = await AuthProvider.AuthenticateAsync(new AuthenticationRequest
            {
                Identifier = result.UserId ?? "",
                Claims = result.UserClaims?.ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, string>()
            });

            if (!authResult.Success)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "فشل إنشاء جلسة المصادقة"
                });
            }

            return Ok(new LoginResponse
            {
                Success = true,
                Token = authResult.AccessToken ?? "",
                ExpiresAt = authResult.ExpiresAt?.DateTime ?? DateTime.UtcNow.AddHours(1),
                Message = "تم التحقق بنجاح"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error verifying two-factor code");
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء التحقق"
            });
        }
    }

    #endregion

    #region OTP Authentication

    /// <summary>
    /// طلب رمز OTP عبر الهاتف
    /// POST /api/auth/otp/phone
    /// </summary>
    [HttpPost("otp/phone")]
    [ProducesResponseType(typeof(OtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OtpResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OtpResponse>> RequestPhoneOtp([FromBody] RequestPhoneOtpRequest request)
    {
        try
        {
            Logger.LogInformation("Requesting phone OTP for: {PhoneNumber}", request.PhoneNumber);

            var result = await TwoFactorProvider.InitiateAsync(
                new TwoFactorInitiationRequest
                {
                    Identifier = request.PhoneNumber,
                    Metadata = new Dictionary<string, string>
                    {
                        ["method"] = "phone"
                    }
                });

            if (!result.Success)
            {
                return BadRequest(new OtpResponse
                {
                    Success = false,
                    Message = result.Error?.Message ?? "فشل إرسال رمز التحقق"
                });
            }

            return Ok(new OtpResponse
            {
                Success = true,
                SessionId = result.TransactionId,
                ExpiresInSeconds = (int?)result.ExpiresIn?.TotalSeconds,
                Message = "تم إرسال رمز التحقق إلى رقم الجوال"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error requesting phone OTP");
            return BadRequest(new OtpResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء إرسال رمز التحقق"
            });
        }
    }

    /// <summary>
    /// طلب رمز OTP عبر البريد الإلكتروني
    /// POST /api/auth/otp/email
    /// </summary>
    [HttpPost("otp/email")]
    [ProducesResponseType(typeof(OtpResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(OtpResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OtpResponse>> RequestEmailOtp([FromBody] RequestEmailOtpRequest request)
    {
        try
        {
            Logger.LogInformation("Requesting email OTP for: {Email}", request.Email);

            var result = await TwoFactorProvider.InitiateAsync(
                new TwoFactorInitiationRequest
                {
                    Identifier = request.Email,
                    Metadata = new Dictionary<string, string>
                    {
                        ["method"] = "email"
                    }
                });

            if (!result.Success)
            {
                return BadRequest(new OtpResponse
                {
                    Success = false,
                    Message = result.Error?.Message ?? "فشل إرسال رمز التحقق"
                });
            }

            return Ok(new OtpResponse
            {
                Success = true,
                SessionId = result.TransactionId,
                ExpiresInSeconds = (int?)result.ExpiresIn?.TotalSeconds,
                Message = "تم إرسال رمز التحقق إلى البريد الإلكتروني"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error requesting email OTP");
            return BadRequest(new OtpResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء إرسال رمز التحقق"
            });
        }
    }

    /// <summary>
    /// التحقق من رمز OTP
    /// POST /api/auth/otp/verify
    /// </summary>
    [HttpPost("otp/verify")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        try
        {
            Logger.LogInformation("Verifying OTP for: {PhoneOrEmail}", request.PhoneOrEmail);

            var result = await TwoFactorProvider.VerifyAsync(
                new TwoFactorVerificationRequest
                {
                    TransactionId = request.PhoneOrEmail, // في حالة OTP، يمكن استخدام الهاتف/البريد كمعرف
                    Code = request.Code
                });

            if (!result.Success)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = result.Error?.Message ?? "رمز التحقق غير صحيح"
                });
            }

            // البحث عن المستخدم أو إنشاء حساب جديد
            // نبحث بالبريد الإلكتروني أو اسم المستخدم (الذي قد يكون رقم الهاتف)
            var user = await _userProvider.GetUserByEmailAsync(request.PhoneOrEmail)
                ?? await _userProvider.GetUserByUsernameAsync(request.PhoneOrEmail);

            string userId;
            if (user != null)
            {
                userId = user.UserId;
            }
            else
            {
                // إنشاء مستخدم جديد تلقائياً
                var createResult = await _userProvider.CreateUserAsync(new CreateUserRequest
                {
                    Username = request.PhoneOrEmail,
                    Email = request.PhoneOrEmail.Contains("@") ? request.PhoneOrEmail : "",
                    PhoneNumber = !request.PhoneOrEmail.Contains("@") ? request.PhoneOrEmail : "",
                    TwoFactorEnabled = true,
                    Roles = ["Customer"]
                });

                if (!createResult.Success || createResult.User == null)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "فشل إنشاء الحساب"
                    });
                }

                userId = createResult.User.UserId;
            }

            // إنشاء التوكن
            var authResult = await AuthProvider.AuthenticateAsync(new AuthenticationRequest
            {
                Identifier = userId,
                Claims = new Dictionary<string, string>
                {
                    [ClaimTypes.NameIdentifier] = userId
                }
            });

            if (!authResult.Success)
            {
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = "فشل إنشاء جلسة المصادقة"
                });
            }

            return Ok(new LoginResponse
            {
                Success = true,
                Token = authResult.AccessToken ?? "",
                ExpiresAt = authResult.ExpiresAt?.DateTime ?? DateTime.UtcNow.AddHours(1),
                Message = "تم التحقق بنجاح"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error verifying OTP");
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "حدث خطأ أثناء التحقق"
            });
        }
    }

    #endregion

    /// <summary>
    /// إنشاء بروفايل للمستخدم الجديد
    /// </summary>
    private async Task CreateProfileForUser(UserInfo user, string fullName, string phoneNumber)
    {
        try
        {
            var profile = new Profile
            {
                Id = Guid.NewGuid(),
                UserId = user.UserId,
                Type = ProfileType.Customer,
                FullName = fullName,
                Email = user.Email,
                PhoneNumber = phoneNumber,
                IsActive = true,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            await _profileRepository.AddAsync(profile);
            await _dbContext.SaveChangesAsync();

            Logger.LogInformation("Profile created for user: {UserId}", user.UserId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating profile for user: {UserId}", user.UserId);
            // Don't throw - profile creation failure shouldn't block registration
        }
    }
}

/// <summary>
/// طلب تحديث التوكن
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

#region Nafath Authentication Models (to match frontend SDK)

/// <summary>
/// طلب بدء مصادقة نفاذ
/// </summary>
public class NafathAuthRequest
{
    public string NationalId { get; set; } = string.Empty;
    public string? RedirectUrl { get; set; }
}

/// <summary>
/// استجابة بدء مصادقة نفاذ
/// </summary>
public class NafathAuthResponse
{
    public bool Success { get; set; }
    public string? SessionId { get; set; }
    public string? TransactionId { get; set; }
    public string? RandomNumber { get; set; }
    public int? ExpiresInSeconds { get; set; }
    public string? Message { get; set; }
    public string? AuthUrl { get; set; }
}

/// <summary>
/// استجابة أرقام الجوال من نفاذ
/// </summary>
public class NafathPhoneNumbersResponse
{
    public List<string> PhoneNumbers { get; set; } = new();
    public string? SessionId { get; set; }
}

/// <summary>
/// طلب اختيار رقم الجوال من نفاذ
/// </summary>
public class SelectNafathNumberRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// طلب إكمال مصادقة نفاذ
/// </summary>
public class CompleteNafathAuthRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? State { get; set; }
}

#endregion

#region Two-Factor Authentication Models

/// <summary>
/// طلب تفعيل/إلغاء المصادقة الثنائية
/// </summary>
public class ToggleTwoFactorRequest
{
    public bool Enable { get; set; }
}

/// <summary>
/// استجابة تفعيل المصادقة الثنائية
/// </summary>
public class TwoFactorResponse
{
    public bool IsEnabled { get; set; }
    public string? QrCodeUrl { get; set; }
    public string? SecretKey { get; set; }
    public string[]? RecoveryCodes { get; set; }
}

/// <summary>
/// طلب التحقق من كود المصادقة الثنائية
/// </summary>
public class VerifyTwoFactorRequest
{
    public string Code { get; set; } = string.Empty;
    public string? PhoneOrEmail { get; set; }
    public string? SessionId { get; set; }
}

#endregion

#region OTP Authentication Models

/// <summary>
/// طلب رمز OTP عبر الهاتف
/// </summary>
public class RequestPhoneOtpRequest
{
    public string PhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// طلب رمز OTP عبر البريد الإلكتروني
/// </summary>
public class RequestEmailOtpRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// التحقق من رمز OTP
/// </summary>
public class VerifyOtpRequest
{
    public string PhoneOrEmail { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// استجابة طلب OTP
/// </summary>
public class OtpResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public int? ExpiresInSeconds { get; set; }
    public string? SessionId { get; set; }
}

#endregion

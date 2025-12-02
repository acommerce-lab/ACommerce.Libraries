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

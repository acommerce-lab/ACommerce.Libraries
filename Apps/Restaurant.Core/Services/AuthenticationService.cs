using Restaurant.Core.DTOs.Auth;
using Restaurant.Core.Entities;
using Restaurant.Core.Enums;

namespace Restaurant.Core.Services;

/// <summary>
/// خدمة المصادقة للمطاعم
/// </summary>
public class AuthenticationService
{
    private readonly Dictionary<string, OtpSession> _otpSessions = new();
    private readonly Dictionary<string, RestaurantUser> _users = new();
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

    public AuthenticationService(string jwtSecret = "RestaurantPlatformSecretKey12345678901234567890",
                                  string jwtIssuer = "RestaurantPlatform",
                                  string jwtAudience = "RestaurantApp")
    {
        _jwtSecret = jwtSecret;
        _jwtIssuer = jwtIssuer;
        _jwtAudience = jwtAudience;
    }

    /// <summary>
    /// إرسال رمز OTP للهاتف
    /// </summary>
    public Task<SendOtpResponse> SendOtpAsync(string phoneNumber, string userType)
    {
        // تنظيف رقم الهاتف
        phoneNumber = NormalizePhoneNumber(phoneNumber);

        // إنشاء جلسة OTP جديدة
        var transactionId = Guid.NewGuid().ToString();
        var code = GenerateOtpCode();

        var session = new OtpSession
        {
            TransactionId = transactionId,
            PhoneNumber = phoneNumber,
            Code = code,
            UserType = userType,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        _otpSessions[transactionId] = session;

        // TODO: إرسال SMS عبر مزود الخدمة
        // في بيئة التطوير، نطبع الكود في Console
        Console.WriteLine($"[OTP] Phone: {phoneNumber}, Code: {code}");

        return Task.FromResult(new SendOtpResponse
        {
            Success = true,
            TransactionId = transactionId,
            Message = "تم إرسال رمز التحقق",
            ExpiresInSeconds = 300
        });
    }

    /// <summary>
    /// التحقق من رمز OTP وتسجيل الدخول
    /// </summary>
    public Task<LoginResponse> VerifyOtpAsync(VerifyOtpRequest request)
    {
        if (!_otpSessions.TryGetValue(request.TransactionId, out var session))
        {
            return Task.FromResult(new LoginResponse
            {
                Success = false,
                Error = "رمز التحقق غير صالح أو منتهي الصلاحية"
            });
        }

        if (session.ExpiresAt < DateTime.UtcNow)
        {
            _otpSessions.Remove(request.TransactionId);
            return Task.FromResult(new LoginResponse
            {
                Success = false,
                Error = "انتهت صلاحية رمز التحقق"
            });
        }

        if (session.Code != request.Code)
        {
            session.Attempts++;
            if (session.Attempts >= 3)
            {
                _otpSessions.Remove(request.TransactionId);
                return Task.FromResult(new LoginResponse
                {
                    Success = false,
                    Error = "تم تجاوز عدد المحاولات المسموح"
                });
            }

            return Task.FromResult(new LoginResponse
            {
                Success = false,
                Error = "رمز التحقق غير صحيح"
            });
        }

        // البحث عن المستخدم أو إنشاء حساب جديد
        var user = GetOrCreateUser(session.PhoneNumber, session.UserType);
        var isNewUser = string.IsNullOrEmpty(user.Name);

        // إنشاء JWT Token
        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddDays(30);

        // تحديث حالة الجلسة
        session.IsVerified = true;
        _otpSessions.Remove(request.TransactionId);

        return Task.FromResult(new LoginResponse
        {
            Success = true,
            Token = token,
            ExpiresAt = expiresAt,
            IsNewUser = isNewUser,
            User = new UserInfo
            {
                Id = user.Id,
                PhoneNumber = user.PhoneNumber,
                Name = user.Name,
                Email = user.Email,
                ImageUrl = user.ImageUrl,
                UserType = user.UserType,
                RestaurantId = user.RestaurantId
            }
        });
    }

    /// <summary>
    /// تسجيل مستخدم جديد (إكمال البيانات)
    /// </summary>
    public Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        // البحث عن الجلسة المحققة
        // في التطبيق الفعلي، نخزن هذا في قاعدة البيانات
        return Task.FromResult(new LoginResponse
        {
            Success = false,
            Error = "يرجى التحقق من الهاتف أولاً"
        });
    }

    /// <summary>
    /// الحصول على معلومات المستخدم من Token
    /// </summary>
    public Task<UserInfo?> GetUserFromTokenAsync(string token)
    {
        try
        {
            var userId = ValidateAndGetUserId(token);
            if (userId == null) return Task.FromResult<UserInfo?>(null);

            if (_users.TryGetValue(userId, out var user))
            {
                return Task.FromResult<UserInfo?>(new UserInfo
                {
                    Id = user.Id,
                    PhoneNumber = user.PhoneNumber,
                    Name = user.Name,
                    Email = user.Email,
                    ImageUrl = user.ImageUrl,
                    UserType = user.UserType,
                    RestaurantId = user.RestaurantId
                });
            }

            return Task.FromResult<UserInfo?>(null);
        }
        catch
        {
            return Task.FromResult<UserInfo?>(null);
        }
    }

    /// <summary>
    /// تحديث معلومات المستخدم
    /// </summary>
    public Task<bool> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        if (!_users.TryGetValue(userId, out var user))
            return Task.FromResult(false);

        if (!string.IsNullOrEmpty(request.Name))
            user.Name = request.Name;
        if (!string.IsNullOrEmpty(request.Email))
            user.Email = request.Email;
        if (!string.IsNullOrEmpty(request.ImageUrl))
            user.ImageUrl = request.ImageUrl;

        return Task.FromResult(true);
    }

    #region Private Methods

    private string NormalizePhoneNumber(string phone)
    {
        // إزالة المسافات والرموز
        phone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

        // تحويل 05 إلى +966 5
        if (phone.StartsWith("05"))
            phone = "+966" + phone.Substring(1);
        else if (phone.StartsWith("5") && phone.Length == 9)
            phone = "+966" + phone;
        else if (!phone.StartsWith("+"))
            phone = "+" + phone;

        return phone;
    }

    private string GenerateOtpCode()
    {
        return new Random().Next(100000, 999999).ToString();
    }

    private RestaurantUser GetOrCreateUser(string phoneNumber, string userType)
    {
        var existingUser = _users.Values.FirstOrDefault(u => u.PhoneNumber == phoneNumber && u.UserType == userType);
        if (existingUser != null)
            return existingUser;

        var newUser = new RestaurantUser
        {
            Id = Guid.NewGuid().ToString(),
            PhoneNumber = phoneNumber,
            UserType = userType,
            CreatedAt = DateTime.UtcNow
        };

        _users[newUser.Id] = newUser;
        return newUser;
    }

    private string GenerateJwtToken(RestaurantUser user)
    {
        // في التطبيق الفعلي، نستخدم JwtSecurityTokenHandler
        // هنا نستخدم تمثيل مبسط
        var payload = $"{user.Id}:{user.PhoneNumber}:{user.UserType}:{DateTime.UtcNow.Ticks}";
        var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
        return $"Bearer.{token}";
    }

    private string? ValidateAndGetUserId(string token)
    {
        if (string.IsNullOrEmpty(token) || !token.StartsWith("Bearer."))
            return null;

        try
        {
            var base64 = token.Substring(7);
            var payload = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            var parts = payload.Split(':');
            return parts.Length >= 1 ? parts[0] : null;
        }
        catch
        {
            return null;
        }
    }

    #endregion
}

/// <summary>
/// جلسة OTP
/// </summary>
internal class OtpSession
{
    public string TransactionId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsVerified { get; set; }
    public int Attempts { get; set; }
}

/// <summary>
/// مستخدم المطعم
/// </summary>
internal class RestaurantUser
{
    public string Id { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? ImageUrl { get; set; }
    public string UserType { get; set; } = string.Empty;
    public Guid? RestaurantId { get; set; }
    public DateTime CreatedAt { get; set; }
}

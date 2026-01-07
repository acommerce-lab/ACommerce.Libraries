using System.Net.Http.Json;
using Restaurant.Core.DTOs.Auth;

namespace Restaurant.Shared.Services;

/// <summary>
/// خدمة المصادقة للتطبيقات
/// </summary>
public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStorage _tokenStorage;

    public event Action? OnAuthStateChanged;

    public AuthService(HttpClient httpClient, ITokenStorage tokenStorage)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_tokenStorage.GetToken());

    public string? CurrentToken => _tokenStorage.GetToken();

    /// <summary>
    /// طلب إرسال رمز OTP
    /// </summary>
    public async Task<SendOtpResponse?> SendOtpAsync(string phoneNumber)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/otp/send", new { phoneNumber });
            return await response.Content.ReadFromJsonAsync<SendOtpResponse>();
        }
        catch (Exception ex)
        {
            return new SendOtpResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    /// <summary>
    /// التحقق من رمز OTP
    /// </summary>
    public async Task<LoginResponse?> VerifyOtpAsync(string transactionId, string code)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/otp/verify", new
            {
                transactionId,
                code
            });

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
            {
                _tokenStorage.SetToken(result.Token, result.ExpiresAt);
                OnAuthStateChanged?.Invoke();
            }

            return result;
        }
        catch (Exception ex)
        {
            return new LoginResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// الحصول على معلومات المستخدم الحالي
    /// </summary>
    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        try
        {
            var token = _tokenStorage.GetToken();
            if (string.IsNullOrEmpty(token))
                return null;

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return await _httpClient.GetFromJsonAsync<UserInfo>("/api/auth/me");
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// تحديث الملف الشخصي
    /// </summary>
    public async Task<bool> UpdateProfileAsync(UpdateProfileRequest request)
    {
        try
        {
            var token = _tokenStorage.GetToken();
            if (string.IsNullOrEmpty(token))
                return false;

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PutAsJsonAsync("/api/auth/profile", request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// تسجيل الخروج
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            var token = _tokenStorage.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                await _httpClient.PostAsync("/api/auth/logout", null);
            }
        }
        catch
        {
            // تجاهل الأخطاء
        }
        finally
        {
            _tokenStorage.ClearToken();
            OnAuthStateChanged?.Invoke();
        }
    }
}

/// <summary>
/// واجهة تخزين التوكن
/// </summary>
public interface ITokenStorage
{
    string? GetToken();
    void SetToken(string token, DateTime? expiresAt = null);
    void ClearToken();
    DateTime? GetTokenExpiry();
}

/// <summary>
/// تخزين التوكن في الذاكرة (للتطوير)
/// </summary>
public class InMemoryTokenStorage : ITokenStorage
{
    private string? _token;
    private DateTime? _expiresAt;

    public string? GetToken() => _token;

    public void SetToken(string token, DateTime? expiresAt = null)
    {
        _token = token;
        _expiresAt = expiresAt;
    }

    public void ClearToken()
    {
        _token = null;
        _expiresAt = null;
    }

    public DateTime? GetTokenExpiry() => _expiresAt;
}

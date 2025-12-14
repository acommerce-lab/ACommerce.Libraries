using ACommerce.Client.Auth.Models;
using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Auth;

/// <summary>
/// Client للمصادقة عبر نفاذ
/// </summary>
public sealed class AuthClient(IApiClient httpClient, TokenManager tokenManager)
{
    private const string ServiceName = "Marketplace";

    #region Nafath Authentication

    /// <summary>
    /// بدء مصادقة نفاذ - يرسل رقم الهوية ويستلم الكود العشوائي
    /// </summary>
    public async Task<NafathAuthResponse?> InitiateNafathAsync(
        string nationalId,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.PostAsync<NafathAuthRequest, NafathAuthResponse>(
            ServiceName,
            "/api/auth/nafath/initiate",
            new NafathAuthRequest { NationalId = nationalId },
            cancellationToken);
    }

    /// <summary>
    /// التحقق من حالة مصادقة نفاذ (polling)
    /// </summary>
    public async Task<NafathStatusResponse?> CheckNafathStatusAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetAsync<NafathStatusResponse>(
            ServiceName,
            $"/api/auth/nafath/status?transactionId={transactionId}",
            cancellationToken);
    }

    /// <summary>
    /// إكمال مصادقة نفاذ بعد نجاح التحقق
    /// </summary>
    public async Task<LoginResponse?> CompleteNafathAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync<CompleteNafathRequest, LoginResponse>(
            ServiceName,
            "/api/auth/nafath/complete",
            new CompleteNafathRequest { TransactionId = transactionId },
            cancellationToken);

        if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
        {
            tokenManager.SetToken(response.Token, response.ExpiresAt);
        }

        return response;
    }

    #endregion

    #region Email/Password Authentication

    /// <summary>
    /// تسجيل الدخول بالبريد الإلكتروني وكلمة المرور
    /// </summary>
    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync<LoginRequest, LoginResponse>(
            ServiceName,
            "/api/auth/login",
            request,
            cancellationToken);

        if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
        {
            tokenManager.SetToken(response.Token, response.ExpiresAt);
        }

        return response;
    }

    /// <summary>
    /// تسجيل مستخدم جديد
    /// </summary>
    public async Task<LoginResponse?> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync<RegisterRequest, LoginResponse>(
            ServiceName,
            "/api/auth/register",
            request,
            cancellationToken);

        if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
        {
            tokenManager.SetToken(response.Token, response.ExpiresAt);
        }

        return response;
    }

    #endregion

    #region OTP Authentication

    /// <summary>
    /// طلب إرسال كود OTP للهاتف
    /// </summary>
    public async Task<OtpResponse?> RequestPhoneOtpAsync(
        RequestPhoneOtpRequest request,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.PostAsync<RequestPhoneOtpRequest, OtpResponse>(
            ServiceName,
            "/api/auth/otp/phone",
            request,
            cancellationToken);
    }

    /// <summary>
    /// طلب إرسال كود OTP للبريد
    /// </summary>
    public async Task<OtpResponse?> RequestEmailOtpAsync(
        RequestEmailOtpRequest request,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.PostAsync<RequestEmailOtpRequest, OtpResponse>(
            ServiceName,
            "/api/auth/otp/email",
            request,
            cancellationToken);
    }

    /// <summary>
    /// التحقق من كود OTP (التحقق الثنائي)
    /// </summary>
    public async Task<LoginResponse?> VerifyTwoFactorAsync(
        VerifyTwoFactorRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync<VerifyTwoFactorRequest, LoginResponse>(
            ServiceName,
            "/api/auth/otp/verify",
            request,
            cancellationToken);

        if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
        {
            tokenManager.SetToken(response.Token, response.ExpiresAt);
        }

        return response;
    }

    #endregion

    #region Extended Nafath Methods

    /// <summary>
    /// بدء مصادقة نفاذ (نسخة موسعة مع إعادة التوجيه)
    /// </summary>
    public async Task<NafathAuthResponse?> InitiateNafathAuthAsync(
        NafathAuthRequest request,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.PostAsync<NafathAuthRequest, NafathAuthResponse>(
            ServiceName,
            "/api/auth/nafath/initiate",
            request,
            cancellationToken);
    }

    /// <summary>
    /// إكمال مصادقة نفاذ (نسخة موسعة)
    /// </summary>
    public async Task<LoginResponse?> CompleteNafathAuthAsync(
        CompleteNafathRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync<CompleteNafathRequest, LoginResponse>(
            ServiceName,
            "/api/auth/nafath/complete",
            request,
            cancellationToken);

        if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
        {
            tokenManager.SetToken(response.Token, response.ExpiresAt);
        }

        return response;
    }

    /// <summary>
    /// إكمال مصادقة نفاذ بعد الـ callback (OAuth-style)
    /// </summary>
    public async Task<LoginResponse?> CompleteNafathAuthAsync(
        CompleteNafathAuthRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync<CompleteNafathAuthRequest, LoginResponse>(
            ServiceName,
            "/api/auth/nafath/callback",
            request,
            cancellationToken);

        if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
        {
            tokenManager.SetToken(response.Token, response.ExpiresAt);
        }

        return response;
    }

    /// <summary>
    /// الحصول على أرقام الهواتف المتاحة من نفاذ
    /// </summary>
    public async Task<NafathPhoneNumbersResponse?> GetNafathPhoneNumbersAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetAsync<NafathPhoneNumbersResponse>(
            ServiceName,
            $"/api/auth/nafath/phone-numbers?transactionId={transactionId}",
            cancellationToken);
    }

    /// <summary>
    /// اختيار رقم الهاتف من نفاذ
    /// </summary>
    public async Task<LoginResponse?> SelectNafathNumberAsync(
        SelectNafathNumberRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync<SelectNafathNumberRequest, LoginResponse>(
            ServiceName,
            "/api/auth/nafath/select-number",
            request,
            cancellationToken);

        if (response?.Success == true && !string.IsNullOrEmpty(response.Token))
        {
            tokenManager.SetToken(response.Token, response.ExpiresAt);
        }

        return response;
    }

    #endregion

    #region User Info

    /// <summary>
    /// الحصول على معلومات البروفايل الحالي
    /// </summary>
    public async Task<ProfileResponse?> GetMeAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetAsync<ProfileResponse>(
            ServiceName,
            "/api/auth/me",
            cancellationToken);
    }

    /// <summary>
    /// تسجيل الخروج
    /// </summary>
    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        // Call server FIRST (while we still have the token)
        try
        {
            await httpClient.PostAsync<object>(
                ServiceName,
                "/api/auth/logout",
                new { },
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Log but don't throw - we still want to clear local token
            Console.WriteLine($"[AuthClient] Logout request failed: {ex.Message}");
        }

        // THEN clear local token
        tokenManager.ClearToken();
    }

    #endregion

    #region Token Management

    /// <summary>
    /// تخزين الـ Token
    /// </summary>
    public void SetToken(string token, DateTime? expiresAt = null) => tokenManager.SetToken(token, expiresAt);

    /// <summary>
    /// تخزين الـ Token (نسخة async)
    /// </summary>
    public Task SetTokenAsync(string token, DateTime? expiresAt = null)
    {
        tokenManager.SetToken(token, expiresAt);
        return Task.CompletedTask;
    }

    /// <summary>
    /// الحصول على الـ Token الحالي
    /// </summary>
    public async Task<string?> GetTokenAsync() => await tokenManager.GetTokenAsync();

    /// <summary>
    /// التحقق من وجود token
    /// </summary>
    public bool HasToken => tokenManager.HasToken;

    /// <summary>
    /// مسح الـ Token
    /// </summary>
    public void ClearToken() => tokenManager.ClearToken();

    #endregion
}

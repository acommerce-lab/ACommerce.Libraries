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

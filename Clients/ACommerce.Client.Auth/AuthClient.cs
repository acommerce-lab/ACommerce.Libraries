using ACommerce.Client.Auth.Models;
using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Auth;

/// <summary>
/// Client للمصادقة عبر نفاذ
/// </summary>
public sealed class AuthClient(IApiClient httpClient)
{
    private const string ServiceName = "Marketplace";
    private string? _currentToken;

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
            _currentToken = response.Token;
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
        _currentToken = null;
        await httpClient.PostAsync<object>(
            ServiceName,
            "/api/auth/logout",
            new { },
            cancellationToken);
    }

    #endregion

    #region Token Management

    /// <summary>
    /// تخزين الـ Token
    /// </summary>
    public void SetToken(string token) => _currentToken = token;

    /// <summary>
    /// الحصول على الـ Token الحالي
    /// </summary>
    public string? GetToken() => _currentToken;

    /// <summary>
    /// التحقق من وجود token
    /// </summary>
    public bool HasToken => !string.IsNullOrEmpty(_currentToken);

    /// <summary>
    /// مسح الـ Token
    /// </summary>
    public void ClearToken() => _currentToken = null;

    #endregion
}

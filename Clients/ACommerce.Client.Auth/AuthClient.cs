using ACommerce.Client.Auth.Models;
using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Auth;

/// <summary>
/// Client للتعامل مع Authentication
/// </summary>
public sealed class AuthClient
{
	private readonly DynamicHttpClient _httpClient;
	private const string ServiceName = "Marketplace"; // أو "Auth" إذا كانت خدمة منفصلة

	public AuthClient(DynamicHttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <summary>
	/// تسجيل دخول
	/// </summary>
	public async Task<LoginResponse?> LoginAsync(
		LoginRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<LoginRequest, LoginResponse>(
			ServiceName,
			"/api/auth/login",
			request,
			cancellationToken);
	}

	/// <summary>
	/// إنشاء حساب جديد
	/// </summary>
	public async Task<LoginResponse?> RegisterAsync(
		RegisterRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<RegisterRequest, LoginResponse>(
			ServiceName,
			"/api/auth/register",
			request,
			cancellationToken);
	}

	/// <summary>
	/// الحصول على معلومات المستخدم الحالي
	/// </summary>
	public async Task<UserInfo?> GetMeAsync(CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<UserInfo>(
			ServiceName,
			"/api/auth/me",
			cancellationToken);
	}

	/// <summary>
	/// تسجيل خروج
	/// </summary>
	public async Task LogoutAsync(CancellationToken cancellationToken = default)
	{
		await _httpClient.PostAsync<object>(
			ServiceName,
			"/api/auth/logout",
			new { },
			cancellationToken);
	}

	/// <summary>
	/// تفعيل/إلغاء المصادقة الثنائية
	/// </summary>
	public async Task<TwoFactorResponse?> ToggleTwoFactorAsync(
		bool enable,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<ToggleTwoFactorRequest, TwoFactorResponse>(
			ServiceName,
			"/api/auth/two-factor",
			new ToggleTwoFactorRequest { Enable = enable },
			cancellationToken);
	}

	/// <summary>
	/// التحقق من كود المصادقة الثنائية
	/// </summary>
	public async Task<LoginResponse?> VerifyTwoFactorAsync(
		string code,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<VerifyTwoFactorRequest, LoginResponse>(
			ServiceName,
			"/api/auth/two-factor/verify",
			new VerifyTwoFactorRequest { Code = code },
			cancellationToken);
	}
}

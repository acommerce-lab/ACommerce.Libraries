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

	// Token storage for client-side apps
	private string? _currentToken;

	public AuthClient(DynamicHttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	#region Basic Authentication

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
		_currentToken = null;
		await _httpClient.PostAsync<object>(
			ServiceName,
			"/api/auth/logout",
			new { },
			cancellationToken);
	}

	#endregion

	#region Two-Factor Authentication

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

	/// <summary>
	/// التحقق من كود المصادقة الثنائية (مع طلب كامل)
	/// </summary>
	public async Task<LoginResponse?> VerifyTwoFactorAsync(
		VerifyTwoFactorRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<VerifyTwoFactorRequest, LoginResponse>(
			ServiceName,
			"/api/auth/two-factor/verify",
			request,
			cancellationToken);
	}

	#endregion

	#region OTP Authentication

	/// <summary>
	/// طلب رمز OTP عبر الهاتف
	/// </summary>
	public async Task<OtpResponse?> RequestPhoneOtpAsync(
		RequestPhoneOtpRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<RequestPhoneOtpRequest, OtpResponse>(
			ServiceName,
			"/api/auth/otp/phone",
			request,
			cancellationToken);
	}

	/// <summary>
	/// طلب رمز OTP عبر البريد الإلكتروني
	/// </summary>
	public async Task<OtpResponse?> RequestEmailOtpAsync(
		RequestEmailOtpRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<RequestEmailOtpRequest, OtpResponse>(
			ServiceName,
			"/api/auth/otp/email",
			request,
			cancellationToken);
	}

	/// <summary>
	/// التحقق من رمز OTP
	/// </summary>
	public async Task<LoginResponse?> VerifyOtpAsync(
		VerifyOtpRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<VerifyOtpRequest, LoginResponse>(
			ServiceName,
			"/api/auth/otp/verify",
			request,
			cancellationToken);
	}

	#endregion

	#region Nafath Authentication (Saudi Arabia)

	/// <summary>
	/// بدء مصادقة نفاذ
	/// </summary>
	public async Task<NafathAuthResponse?> InitiateNafathAuthAsync(
		NafathAuthRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<NafathAuthRequest, NafathAuthResponse>(
			ServiceName,
			"/api/auth/nafath/initiate",
			request,
			cancellationToken);
	}

	/// <summary>
	/// الحصول على أرقام الجوال المسجلة في نفاذ
	/// </summary>
	public async Task<NafathPhoneNumbersResponse?> GetNafathPhoneNumbersAsync(
		string sessionId,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<NafathPhoneNumbersResponse>(
			ServiceName,
			$"/api/auth/nafath/phone-numbers?sessionId={sessionId}",
			cancellationToken);
	}

	/// <summary>
	/// اختيار رقم الجوال من نفاذ
	/// </summary>
	public async Task<LoginResponse?> SelectNafathNumberAsync(
		SelectNafathNumberRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<SelectNafathNumberRequest, LoginResponse>(
			ServiceName,
			"/api/auth/nafath/select-number",
			request,
			cancellationToken);
	}

	/// <summary>
	/// إكمال مصادقة نفاذ
	/// </summary>
	public async Task<LoginResponse?> CompleteNafathAuthAsync(
		CompleteNafathAuthRequest request,
		CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<CompleteNafathAuthRequest, LoginResponse>(
			ServiceName,
			"/api/auth/nafath/complete",
			request,
			cancellationToken);
	}

	#endregion

	#region Token Management

	/// <summary>
	/// تخزين الـ Token (للتطبيقات client-side)
	/// </summary>
	public Task SetTokenAsync(string token)
	{
		_currentToken = token;
		return Task.CompletedTask;
	}

	/// <summary>
	/// الحصول على الـ Token الحالي
	/// </summary>
	public Task<string?> GetTokenAsync()
	{
		return Task.FromResult(_currentToken);
	}

	/// <summary>
	/// التحقق من وجود token
	/// </summary>
	public bool HasToken => !string.IsNullOrEmpty(_currentToken);

	#endregion
}

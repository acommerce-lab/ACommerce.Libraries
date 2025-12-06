using ACommerce.Client.Core.Interceptors;

namespace ACommerce.Client.Auth;

/// <summary>
/// Token Manager للتعامل مع Authentication Token
/// يستخدم ITokenStorage للتخزين الدائم
/// </summary>
public sealed class TokenManager : ITokenProvider
{
	private readonly ITokenStorage _storage;

	// Cache in memory for performance
	private string? _cachedToken;
	private DateTime? _cachedExpiresAt;
	private bool _isInitialized;

	public TokenManager(ITokenStorage storage)
	{
		_storage = storage;
		Console.WriteLine($"[TokenManager] Created with storage: {storage.GetType().Name}");
	}

	/// <summary>
	/// تهيئة المدير واسترجاع التوكن المحفوظ
	/// </summary>
	public async Task InitializeAsync()
	{
		if (_isInitialized) return;

		try
		{
			var (token, expiresAt) = await _storage.GetTokenAsync();
			if (!string.IsNullOrEmpty(token))
			{
				// تحقق من الصلاحية
				if (expiresAt.HasValue && DateTime.UtcNow >= expiresAt.Value)
				{
					Console.WriteLine("[TokenManager] Stored token expired, clearing");
					await _storage.ClearTokenAsync();
				}
				else
				{
					_cachedToken = token;
					_cachedExpiresAt = expiresAt;
					Console.WriteLine($"[TokenManager] Loaded token from storage (expires: {expiresAt})");
				}
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[TokenManager] Error loading token from storage: {ex.Message}");
		}

		_isInitialized = true;
	}

	/// <summary>
	/// حفظ Token
	/// </summary>
	public void SetToken(string token, DateTime? expiresAt = null)
	{
		Console.WriteLine($"[TokenManager] SetToken called - Token: {token.Substring(0, Math.Min(20, token.Length))}...");
		_cachedToken = token;
		_cachedExpiresAt = expiresAt;

		// حفظ في التخزين الدائم
		_ = _storage.SaveTokenAsync(token, expiresAt);
	}

	/// <summary>
	/// الحصول على Token
	/// </summary>
	public async Task<string?> GetTokenAsync()
	{
		// تأكد من التهيئة
		if (!_isInitialized)
		{
			await InitializeAsync();
		}

		// تحقق من الصلاحية
		if (_cachedExpiresAt.HasValue && DateTime.UtcNow >= _cachedExpiresAt.Value)
		{
			Console.WriteLine("[TokenManager] Token expired, clearing");
			await ClearTokenInternalAsync();
			return null;
		}

		return _cachedToken;
	}

	/// <summary>
	/// مسح Token
	/// </summary>
	public void ClearToken()
	{
		_ = ClearTokenInternalAsync();
	}

	private async Task ClearTokenInternalAsync()
	{
		_cachedToken = null;
		_cachedExpiresAt = null;
		await _storage.ClearTokenAsync();
	}

	/// <summary>
	/// هل يوجد Token؟
	/// </summary>
	public bool HasToken => !string.IsNullOrEmpty(_cachedToken);

	/// <summary>
	/// هل Token منتهي الصلاحية؟
	/// </summary>
	public bool IsExpired => _cachedExpiresAt.HasValue && DateTime.UtcNow >= _cachedExpiresAt.Value;

	/// <summary>
	/// استخراج معرف المستخدم من JWT Token
	/// </summary>
	public string? GetUserIdFromToken()
	{
		if (string.IsNullOrEmpty(_cachedToken))
			return null;

		try
		{
			// JWT Token structure: header.payload.signature
			var parts = _cachedToken.Split('.');
			if (parts.Length != 3)
				return null;

			// Decode payload (base64url)
			var payload = parts[1];
			// Add padding if needed
			payload = payload.Replace('-', '+').Replace('_', '/');
			switch (payload.Length % 4)
			{
				case 2: payload += "=="; break;
				case 3: payload += "="; break;
			}

			var jsonBytes = Convert.FromBase64String(payload);
			var json = System.Text.Encoding.UTF8.GetString(jsonBytes);

			// Parse JSON to extract user ID (typically in "sub" or "nameid" claim)
			using var doc = System.Text.Json.JsonDocument.Parse(json);
			var root = doc.RootElement;

			// Try common claim names for user ID
			if (root.TryGetProperty("sub", out var sub))
				return sub.GetString();
			if (root.TryGetProperty("nameid", out var nameid))
				return nameid.GetString();
			if (root.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out var nameidentifier))
				return nameidentifier.GetString();

			return null;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[TokenManager] Error extracting user ID from token: {ex.Message}");
			return null;
		}
	}
}

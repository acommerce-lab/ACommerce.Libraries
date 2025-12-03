using ACommerce.Client.Core.Interceptors;

namespace ACommerce.Client.Auth;

/// <summary>
/// Token Manager للتعامل مع Authentication Token
/// يخزن Token في الذاكرة (يمكن توسيعه للتخزين الدائم)
/// </summary>
public sealed class TokenManager : ITokenProvider
{
	private string? _token;
	private DateTime? _expiresAt;

	public TokenManager()
	{
		Console.WriteLine($"[TokenManager] Created (HashCode: {GetHashCode()})");
	}

	/// <summary>
	/// حفظ Token
	/// </summary>
	public void SetToken(string token, DateTime? expiresAt = null)
	{
		Console.WriteLine($"[TokenManager] SetToken called (HashCode: {GetHashCode()}) - Token: {token.Substring(0, Math.Min(20, token.Length))}...");
		_token = token;
		_expiresAt = expiresAt;
	}

	/// <summary>
	/// الحصول على Token
	/// </summary>
	public Task<string?> GetTokenAsync()
	{
		Console.WriteLine($"[TokenManager] GetTokenAsync called (HashCode: {GetHashCode()}) - HasToken: {!string.IsNullOrEmpty(_token)}");

		// تحقق من الصلاحية
		if (_expiresAt.HasValue && DateTime.UtcNow >= _expiresAt.Value)
		{
			Console.WriteLine($"[TokenManager] Token expired, clearing");
			_token = null;
			_expiresAt = null;
			return Task.FromResult<string?>(null);
		}

		return Task.FromResult(_token);
	}

	/// <summary>
	/// مسح Token
	/// </summary>
	public void ClearToken()
	{
		_token = null;
		_expiresAt = null;
	}

	/// <summary>
	/// هل يوجد Token؟
	/// </summary>
	public bool HasToken => !string.IsNullOrEmpty(_token);

	/// <summary>
	/// هل Token منتهي الصلاحية؟
	/// </summary>
	public bool IsExpired => _expiresAt.HasValue && DateTime.UtcNow >= _expiresAt.Value;

	/// <summary>
	/// استخراج معرف المستخدم من JWT Token
	/// </summary>
	public string? GetUserIdFromToken()
	{
		if (string.IsNullOrEmpty(_token))
			return null;

		try
		{
			// JWT Token structure: header.payload.signature
			var parts = _token.Split('.');
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

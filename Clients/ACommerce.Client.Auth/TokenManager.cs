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
}

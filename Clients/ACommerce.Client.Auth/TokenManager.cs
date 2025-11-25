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

	/// <summary>
	/// حفظ Token
	/// </summary>
	public void SetToken(string token, DateTime? expiresAt = null)
	{
		_token = token;
		_expiresAt = expiresAt;
	}

	/// <summary>
	/// الحصول على Token
	/// </summary>
	public Task<string?> GetTokenAsync()
	{
		// تحقق من الصلاحية
		if (_expiresAt.HasValue && DateTime.UtcNow >= _expiresAt.Value)
		{
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

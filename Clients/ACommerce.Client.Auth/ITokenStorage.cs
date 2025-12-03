namespace ACommerce.Client.Auth;

/// <summary>
/// واجهة لتخزين التوكن بشكل دائم
/// يتم تنفيذها بشكل مختلف حسب المنصة (MAUI, Web, etc.)
/// </summary>
public interface ITokenStorage
{
	/// <summary>
	/// حفظ التوكن
	/// </summary>
	Task SaveTokenAsync(string token, DateTime? expiresAt);

	/// <summary>
	/// استرجاع التوكن
	/// </summary>
	Task<(string? Token, DateTime? ExpiresAt)> GetTokenAsync();

	/// <summary>
	/// حذف التوكن
	/// </summary>
	Task ClearTokenAsync();

	/// <summary>
	/// هل يوجد توكن محفوظ؟
	/// </summary>
	Task<bool> HasTokenAsync();
}

/// <summary>
/// تخزين في الذاكرة فقط (الافتراضي - لا يحفظ بين الجلسات)
/// </summary>
public class InMemoryTokenStorage : ITokenStorage
{
	private string? _token;
	private DateTime? _expiresAt;

	public Task SaveTokenAsync(string token, DateTime? expiresAt)
	{
		_token = token;
		_expiresAt = expiresAt;
		return Task.CompletedTask;
	}

	public Task<(string? Token, DateTime? ExpiresAt)> GetTokenAsync()
	{
		return Task.FromResult((_token, _expiresAt));
	}

	public Task ClearTokenAsync()
	{
		_token = null;
		_expiresAt = null;
		return Task.CompletedTask;
	}

	public Task<bool> HasTokenAsync()
	{
		return Task.FromResult(!string.IsNullOrEmpty(_token));
	}
}

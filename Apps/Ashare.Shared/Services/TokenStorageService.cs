using System.Text.Json;
using ACommerce.Client.Auth;

namespace Ashare.Shared.Services;

/// <summary>
/// تنفيذ ITokenStorage باستخدام IStorageService
/// يعمل مع أي تنفيذ لـ IStorageService (MAUI SecureStorage, Web localStorage, etc.)
/// </summary>
public class TokenStorageService : ITokenStorage
{
	private const string TokenKey = "auth_token";
	private const string ExpiresAtKey = "auth_expires_at";

	private readonly IStorageService _storage;

	/// <summary>
	/// Static event fired when token changes - allows ScopedTokenProvider to update its cache
	/// </summary>
	public static event Action<string?, DateTime?>? OnTokenChanged;

	public TokenStorageService(IStorageService storage)
	{
		_storage = storage;
	}

	public async Task SaveTokenAsync(string token, DateTime? expiresAt)
	{
		// Notify subscribers IMMEDIATELY (before async storage save)
		// This ensures ScopedTokenProvider's cache is updated before any requests are made
		OnTokenChanged?.Invoke(token, expiresAt);
		Console.WriteLine("[TokenStorageService] OnTokenChanged fired immediately");

		try
		{
			await _storage.SetAsync(TokenKey, token);

			if (expiresAt.HasValue)
			{
				await _storage.SetAsync(ExpiresAtKey, expiresAt.Value.ToString("O"));
			}
			else
			{
				await _storage.RemoveAsync(ExpiresAtKey);
			}

			Console.WriteLine("[TokenStorageService] Token saved to persistent storage");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[TokenStorageService] Error saving token: {ex.Message}");
		}
	}

	public async Task<(string? Token, DateTime? ExpiresAt)> GetTokenAsync()
	{
		try
		{
			var token = await _storage.GetAsync(TokenKey);
			DateTime? expiresAt = null;

			var expiresAtStr = await _storage.GetAsync(ExpiresAtKey);
			if (!string.IsNullOrEmpty(expiresAtStr) && DateTime.TryParse(expiresAtStr, out var parsed))
			{
				expiresAt = parsed;
			}

			if (!string.IsNullOrEmpty(token))
			{
				Console.WriteLine($"[TokenStorageService] Token loaded from persistent storage (expires: {expiresAt})");
				// Notify subscribers that we loaded a token - updates ScopedTokenProvider's cache
				OnTokenChanged?.Invoke(token, expiresAt);
				Console.WriteLine("[TokenStorageService] OnTokenChanged fired after loading token");
			}

			return (token, expiresAt);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[TokenStorageService] Error loading token: {ex.Message}");
			return (null, null);
		}
	}

	public async Task ClearTokenAsync()
	{
		// Notify subscribers IMMEDIATELY (before async storage clear)
		OnTokenChanged?.Invoke(null, null);
		Console.WriteLine("[TokenStorageService] OnTokenChanged (clear) fired immediately");

		try
		{
			await _storage.RemoveAsync(TokenKey);
			await _storage.RemoveAsync(ExpiresAtKey);
			Console.WriteLine("[TokenStorageService] Token cleared from persistent storage");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[TokenStorageService] Error clearing token: {ex.Message}");
		}
	}

	public async Task<bool> HasTokenAsync()
	{
		var token = await _storage.GetAsync(TokenKey);
		return !string.IsNullOrEmpty(token);
	}
}

using ACommerce.Notifications.Channels.Firebase.Models;
using System.Collections.Concurrent;

namespace ACommerce.Notifications.Channels.Firebase.Storage;

/// <summary>
/// ????? ???? ??? Tokens ?? ??????? (??????? ?????????)
/// ?? ???????? ?????? Database (Redis/SQL)
/// </summary>
public class InMemoryFirebaseTokenStore : IFirebaseTokenStore
{
	private readonly ConcurrentDictionary<string, FirebaseDeviceToken> _tokens = new();

	public Task SaveTokenAsync(
		FirebaseDeviceToken deviceToken,
		CancellationToken cancellationToken = default)
	{
		_tokens.AddOrUpdate(
			deviceToken.Token,
			deviceToken,
			(_, existing) =>
			{
				// Create a new instance with updated LastUsedAt, copying other properties
				return new FirebaseDeviceToken
				{
					UserId = deviceToken.UserId,
					Token = deviceToken.Token,
					Platform = deviceToken.Platform,
					RegisteredAt = deviceToken.RegisteredAt,
					LastUsedAt = DateTimeOffset.UtcNow,
					IsActive = deviceToken.IsActive,
					Metadata = deviceToken.Metadata
				};
			});

		return Task.CompletedTask;
	}

	public Task<List<FirebaseDeviceToken>> GetUserTokensAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		var tokens = _tokens.Values
			.Where(t => t.UserId == userId && t.IsActive)
			.ToList();

		return Task.FromResult(tokens);
	}

	public Task DeleteTokenAsync(
		string token,
		CancellationToken cancellationToken = default)
	{
		_tokens.TryRemove(token, out _);
		return Task.CompletedTask;
	}

	public Task DeactivateTokenAsync(
		string token,
		CancellationToken cancellationToken = default)
	{
		if (_tokens.TryGetValue(token, out var deviceToken))
		{
			deviceToken.IsActive = false;
		}

		return Task.CompletedTask;
	}

	public Task<int> GetActiveDeviceCountAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		var count = _tokens.Values
			.Count(t => t.UserId == userId && t.IsActive);

		return Task.FromResult(count);
	}
}


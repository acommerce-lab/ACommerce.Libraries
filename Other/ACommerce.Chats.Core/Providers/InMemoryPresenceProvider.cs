using ACommerce.Chats.Abstractions.Providers;
using System.Collections.Concurrent;

namespace ACommerce.Chats.Core.Providers;

/// <summary>
/// ???? Presence ???? ???????? ???????
/// ???????: ?????? Redis ?? Database
/// </summary>
public class InMemoryPresenceProvider : IPresenceProvider
{
	private readonly ConcurrentDictionary<string, bool> _presenceCache = new();

	public Task UpdateUserPresenceAsync(
		string userId,
		bool isOnline,
		CancellationToken cancellationToken = default)
	{
		_presenceCache[userId] = isOnline;
		return Task.CompletedTask;
	}

	public Task<bool> GetUserPresenceAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_presenceCache.GetValueOrDefault(userId, false));
	}

	public Task<Dictionary<string, bool>> GetUsersPresenceAsync(
		List<string> userIds,
		CancellationToken cancellationToken = default)
	{
		var result = userIds.ToDictionary(
			userId => userId,
			userId => _presenceCache.GetValueOrDefault(userId, false));

		return Task.FromResult(result);
	}
}


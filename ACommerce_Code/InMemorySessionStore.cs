// ACommerce.Authentication.TwoFactor.Abstractions/InMemorySessionStore.cs
using System.Collections.Concurrent;

namespace ACommerce.Authentication.TwoFactor.Abstractions;

/// <summary>
/// In-memory session store for development/testing
/// For production, use Redis or Database implementation
/// </summary>
public class InMemoryTwoFactorSessionStore : ITwoFactorSessionStore
{
	private readonly ConcurrentDictionary<string, TwoFactorSession> _sessions = new();

	public Task<string> CreateSessionAsync(
		TwoFactorSession session,
		CancellationToken cancellationToken = default)
	{
		_sessions[session.TransactionId] = session;
		return Task.FromResult(session.TransactionId);
	}

	public Task<TwoFactorSession?> GetSessionAsync(
		string transactionId,
		CancellationToken cancellationToken = default)
	{
		_sessions.TryGetValue(transactionId, out var session);
		return Task.FromResult(session);
	}

	public Task UpdateSessionAsync(
		TwoFactorSession session,
		CancellationToken cancellationToken = default)
	{
		_sessions[session.TransactionId] = session;
		return Task.CompletedTask;
	}

	public Task DeleteSessionAsync(
		string transactionId,
		CancellationToken cancellationToken = default)
	{
		_sessions.TryRemove(transactionId, out _);
		return Task.CompletedTask;
	}
}


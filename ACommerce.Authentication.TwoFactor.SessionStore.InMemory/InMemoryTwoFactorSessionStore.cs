using ACommerce.Authentication.TwoFactor.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ACommerce.Authentication.TwoFactor.SessionStore.InMemory;

/// <summary>
/// In-memory implementation of ITwoFactorSessionStore
/// WARNING: Use only for development/testing. Data is lost on restart.
/// </summary>
public class InMemoryTwoFactorSessionStore(
    ILogger<InMemoryTwoFactorSessionStore> logger)
    : ITwoFactorSessionStore
{
    private readonly ConcurrentDictionary<string, TwoFactorSession> _sessions = new();

    public Task<TwoFactorSession?> GetSessionAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("[InMemory] Getting session {TransactionId}", transactionId);

        if (_sessions.TryGetValue(transactionId, out var session))
        {
            // Check if expired
            if (session.ExpiresAt < DateTimeOffset.UtcNow)
            {
                logger.LogWarning("[InMemory] Session {TransactionId} has expired", transactionId);
                _sessions.TryRemove(transactionId, out _);
                return Task.FromResult<TwoFactorSession?>(null);
            }

            return Task.FromResult<TwoFactorSession?>(session);
        }

        logger.LogWarning("[InMemory] Session {TransactionId} not found", transactionId);
        return Task.FromResult<TwoFactorSession?>(null);
    }

    public Task<string> CreateSessionAsync(
        TwoFactorSession session,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[InMemory] Creating session {TransactionId} for {Identifier}",
            session.TransactionId,
            session.Identifier);

        if (!_sessions.TryAdd(session.TransactionId, session))
        {
            logger.LogError(
                "[InMemory] Failed to create session {TransactionId} - already exists",
                session.TransactionId);
            throw new InvalidOperationException(
                $"Session {session.TransactionId} already exists");
        }

        return Task.FromResult(session.TransactionId);
    }

    public Task UpdateSessionAsync(
        TwoFactorSession session,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[InMemory] Updating session {TransactionId} to status {Status}",
            session.TransactionId,
            session.Status);

        if (!_sessions.TryGetValue(session.TransactionId, out _))
        {
            logger.LogError(
                "[InMemory] Cannot update session {TransactionId} - not found",
                session.TransactionId);
            throw new InvalidOperationException(
                $"Session {session.TransactionId} not found");
        }

        _sessions[session.TransactionId] = session;
        return Task.CompletedTask;
    }

    public Task DeleteSessionAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[InMemory] Deleting session {TransactionId}", transactionId);

        _sessions.TryRemove(transactionId, out _);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Clean up expired sessions
    /// Call this periodically in a background service
    /// </summary>
    public int CleanupExpiredSessions()
    {
        var now = DateTimeOffset.UtcNow;
        var expiredKeys = _sessions
            .Where(kvp => kvp.Value.ExpiresAt < now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _sessions.TryRemove(key, out _);
        }

        if (expiredKeys.Count > 0)
        {
            logger.LogInformation(
                "[InMemory] Cleaned up {Count} expired sessions",
                expiredKeys.Count);
        }

        return expiredKeys.Count;
    }

    /// <summary>
    /// Get total number of sessions (for monitoring)
    /// </summary>
    public int GetSessionCount() => _sessions.Count;
}

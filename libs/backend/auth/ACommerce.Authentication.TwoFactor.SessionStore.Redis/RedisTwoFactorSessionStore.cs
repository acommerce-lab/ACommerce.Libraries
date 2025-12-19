using ACommerce.Authentication.TwoFactor.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace ACommerce.Authentication.TwoFactor.SessionStore.Redis;

public class RedisTwoFactorSessionStore : ITwoFactorSessionStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisTwoFactorSessionStore> _logger;
    private readonly RedisSessionStoreOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisTwoFactorSessionStore(
        IConnectionMultiplexer redis,
        IOptions<RedisSessionStoreOptions> options,
        ILogger<RedisTwoFactorSessionStore> logger)
    {
        _redis = redis;
        _logger = logger;
        _options = options.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    private string GetKey(string transactionId) => $"{_options.KeyPrefix}{transactionId}";

    public async Task<string> CreateSessionAsync(
        TwoFactorSession session,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(session.TransactionId);
        var json = JsonSerializer.Serialize(session, _jsonOptions);
        var expiry = session.ExpiresAt - DateTimeOffset.UtcNow;

        if (expiry <= TimeSpan.Zero)
        {
            expiry = TimeSpan.FromMinutes(_options.DefaultExpirationMinutes);
        }

        var success = await db.StringSetAsync(key, json, expiry, When.NotExists);

        if (!success)
        {
            _logger.LogError("[Redis] Failed to create session {TransactionId} - already exists", session.TransactionId);
            throw new InvalidOperationException($"Session {session.TransactionId} already exists");
        }

        _logger.LogInformation(
            "[Redis] Created session {TransactionId} for {Identifier}, expires in {Expiry}",
            session.TransactionId, session.Identifier, expiry);

        return session.TransactionId;
    }

    public async Task<TwoFactorSession?> GetSessionAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(transactionId);
        var value = await db.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            _logger.LogWarning("[Redis] Session {TransactionId} not found", transactionId);
            return null;
        }

        try
        {
            var session = JsonSerializer.Deserialize<TwoFactorSession>(value!, _jsonOptions);

            if (session != null && session.ExpiresAt < DateTimeOffset.UtcNow)
            {
                _logger.LogWarning("[Redis] Session {TransactionId} has expired", transactionId);
                await db.KeyDeleteAsync(key);
                return null;
            }

            _logger.LogDebug("[Redis] Retrieved session {TransactionId}", transactionId);
            return session;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "[Redis] Failed to deserialize session {TransactionId}", transactionId);
            return null;
        }
    }

    public async Task UpdateSessionAsync(
        TwoFactorSession session,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(session.TransactionId);

        var exists = await db.KeyExistsAsync(key);
        if (!exists)
        {
            _logger.LogError("[Redis] Cannot update session {TransactionId} - not found", session.TransactionId);
            throw new InvalidOperationException($"Session {session.TransactionId} not found");
        }

        var json = JsonSerializer.Serialize(session, _jsonOptions);
        
        var expiry = session.ExpiresAt - DateTimeOffset.UtcNow;
        if (expiry <= TimeSpan.Zero)
        {
            expiry = TimeSpan.FromMinutes(_options.DefaultExpirationMinutes);
        }

        await db.StringSetAsync(key, json, expiry);

        _logger.LogInformation(
            "[Redis] Updated session {TransactionId} to status {Status}, expires in {Expiry}",
            session.TransactionId, session.Status, expiry);
    }

    public async Task DeleteSessionAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = GetKey(transactionId);

        await db.KeyDeleteAsync(key);

        _logger.LogInformation("[Redis] Deleted session {TransactionId}", transactionId);
    }
}

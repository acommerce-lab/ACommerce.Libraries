using System.Collections.Concurrent;
using ACommerce.Authentication.Operations.Abstractions;
using ACommerce.Authentication.Providers.Token;

namespace Ashare.Api2.Services;

/// <summary>
/// مخزن رموز بسيط في الذاكرة + مُولّد + مُتحقق.
/// يستخدم Guid كرمز للبساطة (لا JWT).
/// </summary>
public class AshareTokenStore : ITokenValidator, ITokenIssuer
{
    private record TokenEntry(string AccessToken, string RefreshToken, string UserId, DateTimeOffset ExpiresAt);

    private readonly ConcurrentDictionary<string, TokenEntry> _byAccess = new();
    private readonly ConcurrentDictionary<string, TokenEntry> _byRefresh = new();

    public Task<TokenValidationResult> ValidateAsync(string token, CancellationToken ct = default)
    {
        if (!_byAccess.TryGetValue(token, out var entry))
            throw new AuthenticationException("invalid_token", "Token not found");

        if (entry.ExpiresAt <= DateTimeOffset.UtcNow)
            throw new AuthenticationException("expired", "Token expired");

        return Task.FromResult(new TokenValidationResult(
            UserId: entry.UserId,
            DisplayName: entry.UserId,
            Claims: new Dictionary<string, string> { ["user_id"] = entry.UserId },
            ExpiresAt: entry.ExpiresAt));
    }

    public Task<AuthToken> IssueAsync(IPrincipal principal, CancellationToken ct = default)
    {
        var access = Guid.NewGuid().ToString("N");
        var refresh = Guid.NewGuid().ToString("N");
        var expires = DateTimeOffset.UtcNow.AddHours(2);

        var entry = new TokenEntry(access, refresh, principal.UserId, expires);
        _byAccess[access] = entry;
        _byRefresh[refresh] = entry;

        return Task.FromResult(new AuthToken(access, refresh, expires));
    }

    public Task<AuthToken> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        if (!_byRefresh.TryRemove(refreshToken, out var oldEntry))
            throw new AuthenticationException("invalid_refresh_token", "Refresh token not found");

        _byAccess.TryRemove(oldEntry.AccessToken, out _);

        return IssueAsync(new AsharePrincipal { UserId = oldEntry.UserId }, ct);
    }

    public Task RevokeAsync(string token, CancellationToken ct = default)
    {
        if (_byAccess.TryRemove(token, out var entry))
            _byRefresh.TryRemove(entry.RefreshToken, out _);
        return Task.CompletedTask;
    }
}

// ACommerce.Authentication.JWT/InMemoryClaimProvider.cs
using ACommerce.Authentication.Users.Abstractions;
using System.Collections.Concurrent;

namespace ACommerce.Authentication.JWT;

public class InMemoryClaimProvider : IClaimProvider
{
	private readonly ConcurrentDictionary<string, Dictionary<string, string>> _userClaims = new();
	private readonly ConcurrentDictionary<string, Dictionary<string, string>> _roleClaims = new();

	public string ProviderName => "InMemory-JWT";

	public Task<bool> AddUserClaimAsync(
		string userId,
		string claimType,
		string claimValue,
		CancellationToken cancellationToken = default)
	{
		if (!_userClaims.ContainsKey(userId))
			_userClaims[userId] = new Dictionary<string, string>();

		_userClaims[userId][claimType] = claimValue;
		return Task.FromResult(true);
	}

	public Task<bool> RemoveUserClaimAsync(
		string userId,
		string claimType,
		string claimValue,
		CancellationToken cancellationToken = default)
	{
		if (_userClaims.TryGetValue(userId, out var claims))
		{
			claims.Remove(claimType);
			return Task.FromResult(true);
		}
		return Task.FromResult(false);
	}

	public Task<Dictionary<string, string>> GetUserClaimsAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		_userClaims.TryGetValue(userId, out var claims);
		return Task.FromResult(claims ?? new Dictionary<string, string>());
	}

	public Task<bool> AddRoleClaimAsync(
		string roleName,
		string claimType,
		string claimValue,
		CancellationToken cancellationToken = default)
	{
		if (!_roleClaims.ContainsKey(roleName))
			_roleClaims[roleName] = new Dictionary<string, string>();

		_roleClaims[roleName][claimType] = claimValue;
		return Task.FromResult(true);
	}

	public Task<bool> RemoveRoleClaimAsync(
		string roleName,
		string claimType,
		string claimValue,
		CancellationToken cancellationToken = default)
	{
		if (_roleClaims.TryGetValue(roleName, out var claims))
		{
			claims.Remove(claimType);
			return Task.FromResult(true);
		}
		return Task.FromResult(false);
	}

	public Task<Dictionary<string, string>> GetRoleClaimsAsync(
		string roleName,
		CancellationToken cancellationToken = default)
	{
		_roleClaims.TryGetValue(roleName, out var claims);
		return Task.FromResult(claims ?? new Dictionary<string, string>());
	}

	public Task<Dictionary<string, string>> GetAllUserClaimsAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		// ??????? - ?? ??????? ????? User Claims + Role Claims
		return GetUserClaimsAsync(userId, cancellationToken);
	}

	public Task<bool> HasClaimAsync(
		string userId,
		string claimType,
		string claimValue,
		CancellationToken cancellationToken = default)
	{
		if (_userClaims.TryGetValue(userId, out var claims))
		{
			return Task.FromResult(
				claims.TryGetValue(claimType, out var value) && value == claimValue);
		}
		return Task.FromResult(false);
	}
}


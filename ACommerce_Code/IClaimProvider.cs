// Providers/IClaimProvider.cs
namespace ACommerce.Authentication.Users.Abstractions;

/// <summary>
/// ???? ????? ??? Claims
/// </summary>
public interface IClaimProvider
{
	string ProviderName { get; }

	Task<bool> AddUserClaimAsync(
		string userId,
		string claimType,
		string claimValue,
		CancellationToken cancellationToken = default);

	Task<bool> RemoveUserClaimAsync(
		string userId,
		string claimType,
		string claimValue,
		CancellationToken cancellationToken = default);

	Task<Dictionary<string, string>> GetUserClaimsAsync(
		string userId,
		CancellationToken cancellationToken = default);

	Task<bool> AddRoleClaimAsync(
		string roleName,
		string claimType,
		string claimValue,
		CancellationToken cancellationToken = default);

	Task<bool> RemoveRoleClaimAsync(
		string roleName,
		string claimType,
		string claimValue,
		CancellationToken cancellationToken = default);

	Task<Dictionary<string, string>> GetRoleClaimsAsync(
		string roleName,
		CancellationToken cancellationToken = default);

	Task<Dictionary<string, string>> GetAllUserClaimsAsync(
		string userId,
		CancellationToken cancellationToken = default);

	Task<bool> HasClaimAsync(
		string userId,
		string claimType,
		string claimValue,
		CancellationToken cancellationToken = default);
}


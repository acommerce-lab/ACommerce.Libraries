// Providers/IRoleProvider.cs
using ACommerce.Authentication.Users.Abstractions.Models;
using System.IO;

namespace ACommerce.Authentication.Users.Abstractions;

/// <summary>
/// ???? ????? ???????
/// </summary>
public interface IRoleProvider
{
	string ProviderName { get; }

	Task<RoleResult> CreateRoleAsync(
		CreateRoleRequest request,
		CancellationToken cancellationToken = default);

	Task<RoleInfo?> GetRoleByIdAsync(
		string roleId,
		CancellationToken cancellationToken = default);

	Task<RoleInfo?> GetRoleByNameAsync(
		string roleName,
		CancellationToken cancellationToken = default);

	Task<List<RoleInfo>> GetAllRolesAsync(
		CancellationToken cancellationToken = default);

	Task<RoleResult> UpdateRoleAsync(
		string roleId,
		UpdateRoleRequest request,
		CancellationToken cancellationToken = default);

	Task<bool> DeleteRoleAsync(
		string roleId,
		CancellationToken cancellationToken = default);

	Task<bool> AddUserToRoleAsync(
		string userId,
		string roleName,
		CancellationToken cancellationToken = default);

	Task<bool> RemoveUserFromRoleAsync(
		string userId,
		string roleName,
		CancellationToken cancellationToken = default);

	Task<List<string>> GetUserRolesAsync(
		string userId,
		CancellationToken cancellationToken = default);

	Task<bool> IsUserInRoleAsync(
		string userId,
		string roleName,
		CancellationToken cancellationToken = default);

	Task<List<UserInfo>> GetUsersInRoleAsync(
		string roleName,
		CancellationToken cancellationToken = default);
}


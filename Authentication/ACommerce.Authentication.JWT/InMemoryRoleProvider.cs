// ACommerce.Authentication.JWT/InMemoryRoleProvider.cs
using ACommerce.Authentication.Users.Abstractions;
using ACommerce.Authentication.Users.Abstractions.Models;
using System.Collections.Concurrent;

namespace ACommerce.Authentication.JWT;

public class InMemoryRoleProvider : IRoleProvider
{
	private readonly ConcurrentDictionary<string, InMemoryRole> _roles = new();
	private readonly ConcurrentDictionary<string, List<string>> _userRoles = new();

	public string ProviderName => "InMemory-JWT";

	public Task<RoleResult> CreateRoleAsync(
		CreateRoleRequest request,
		CancellationToken cancellationToken = default)
	{
		var roleId = Guid.NewGuid().ToString();
		var role = new InMemoryRole
		{
			RoleId = roleId,
			RoleName = request.Name,
			Description = request.Description,
			IsActive = true,
			Claims = new Dictionary<string, string>(),
			Metadata = request.Metadata ?? new Dictionary<string, string>()
		};

		_roles[roleId] = role;

		return Task.FromResult(new RoleResult
		{
			Success = true,
			Role = MapToRoleInfo(role)
		});
	}

	public Task<RoleInfo?> GetRoleByIdAsync(
		string roleId,
		CancellationToken cancellationToken = default)
	{
		_roles.TryGetValue(roleId, out var role);
		return Task.FromResult(role != null ? MapToRoleInfo(role) : null);
	}

	public Task<RoleInfo?> GetRoleByNameAsync(
		string roleName,
		CancellationToken cancellationToken = default)
	{
		var role = _roles.Values.FirstOrDefault(r =>
			r.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
		return Task.FromResult(role != null ? MapToRoleInfo(role) : null);
	}

	public Task<List<RoleInfo>> GetAllRolesAsync(
		CancellationToken cancellationToken = default)
	{
		var roles = _roles.Values.Select(MapToRoleInfo).ToList();
		return Task.FromResult(roles);
	}

	public Task<RoleResult> UpdateRoleAsync(
		string roleId,
		UpdateRoleRequest request,
		CancellationToken cancellationToken = default)
	{
		if (!_roles.TryGetValue(roleId, out var role))
		{
			return Task.FromResult(new RoleResult
			{
				Success = false,
				Error = new RoleError { Code = "ROLE_NOT_FOUND", Message = "Role not found" }
			});
		}

		if (request.Name != null) role.RoleName = request.Name;
		if (request.Description != null) role.Description = request.Description;
		if (request.IsActive.HasValue) role.IsActive = request.IsActive.Value;
		if (request.Metadata != null) role.Metadata = request.Metadata;

		return Task.FromResult(new RoleResult { Success = true, Role = MapToRoleInfo(role) });
	}

	public Task<bool> DeleteRoleAsync(
		string roleId,
		CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_roles.TryRemove(roleId, out _));
	}

	public Task<bool> AddUserToRoleAsync(
		string userId,
		string roleName,
		CancellationToken cancellationToken = default)
	{
		if (!_userRoles.ContainsKey(userId))
			_userRoles[userId] = new List<string>();

		if (!_userRoles[userId].Contains(roleName, StringComparer.OrdinalIgnoreCase))
			_userRoles[userId].Add(roleName);

		return Task.FromResult(true);
	}

	public Task<bool> RemoveUserFromRoleAsync(
		string userId,
		string roleName,
		CancellationToken cancellationToken = default)
	{
		if (_userRoles.TryGetValue(userId, out var roles))
		{
			roles.RemoveAll(r => r.Equals(roleName, StringComparison.OrdinalIgnoreCase));
			return Task.FromResult(true);
		}
		return Task.FromResult(false);
	}

	public Task<List<string>> GetUserRolesAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		_userRoles.TryGetValue(userId, out var roles);
		return Task.FromResult(roles ?? new List<string>());
	}

	public Task<bool> IsUserInRoleAsync(
		string userId,
		string roleName,
		CancellationToken cancellationToken = default)
	{
		if (_userRoles.TryGetValue(userId, out var roles))
			return Task.FromResult(roles.Contains(roleName, StringComparer.OrdinalIgnoreCase));
		return Task.FromResult(false);
	}

	public Task<List<UserInfo>> GetUsersInRoleAsync(
		string roleName,
		CancellationToken cancellationToken = default)
	{
		// ??????? - ?? ??????? ?????? UserProvider
		return Task.FromResult(new List<UserInfo>());
	}

	private RoleInfo MapToRoleInfo(InMemoryRole role)
	{
		return new RoleInfo
		{
			RoleId = role.RoleId,
			RoleName = role.RoleName,
			Description = role.Description,
			IsActive = role.IsActive,
			Claims = role.Claims,
			Metadata = role.Metadata
		};
	}

	private class InMemoryRole
	{
		public string RoleId { get; set; } = default!;
		public string RoleName { get; set; } = default!;
		public string? Description { get; set; }
		public bool IsActive { get; set; }
		public Dictionary<string, string> Claims { get; set; } = new();
		public Dictionary<string, string> Metadata { get; set; } = new();
	}
}


// ACommerce.Authentication.JWT/InMemoryUserProvider.cs
using ACommerce.Authentication.Users.Abstractions;
using ACommerce.Authentication.Users.Abstractions.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace ACommerce.Authentication.JWT;

/// <summary>
/// In-memory user provider for JWT (??????? ??????)
/// For production, use database implementation
/// </summary>
public class InMemoryUserProvider : IUserProvider
{
	private readonly ConcurrentDictionary<string, InMemoryUser> _users = new();
	public string ProviderName => "InMemory-JWT";

	public Task<UserResult> CreateUserAsync(
		CreateUserRequest request,
		CancellationToken cancellationToken = default)
	{
		var userId = Guid.NewGuid().ToString();

		var user = new InMemoryUser
		{
			UserId = userId,
			Username = request.Username,
			Email = request.Email,
			PhoneNumber = request.PhoneNumber,
			PasswordHash = HashPassword(request.Password),
			EmailVerified = !request.RequireEmailConfirmation,
			PhoneNumberVerified = false,
			TwoFactorEnabled = request.TwoFactorEnabled,
			IsActive = true,
			IsLocked = false,
			Roles = request.Roles?.ToList() ?? [],
			Claims = [],
			Metadata = request.Metadata ?? []
		};

		_users[userId] = user;

		return Task.FromResult(new UserResult
		{
			Success = true,
			User = MapToUserInfo(user)
		});
	}

	public Task<UserInfo?> GetUserByIdAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		_users.TryGetValue(userId, out var user);
		return Task.FromResult(user != null ? MapToUserInfo(user) : null);
	}

	public Task<UserInfo?> GetUserByEmailAsync(
		string email,
		CancellationToken cancellationToken = default)
	{
		var user = _users.Values.FirstOrDefault(u =>
			u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
		return Task.FromResult(user != null ? MapToUserInfo(user) : null);
	}

	public Task<UserInfo?> GetUserByUsernameAsync(
		string username,
		CancellationToken cancellationToken = default)
	{
		var user = _users.Values.FirstOrDefault(u =>
			u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
		return Task.FromResult(user != null ? MapToUserInfo(user) : null);
	}

	public Task<UserResult> UpdateUserAsync(
		string userId,
		UpdateUserRequest request,
		CancellationToken cancellationToken = default)
	{
		if (!_users.TryGetValue(userId, out var user))
		{
			return Task.FromResult(new UserResult
			{
				Success = false,
				Error = new UserError
				{
					Code = "USER_NOT_FOUND",
					Message = "User not found"
				}
			});
		}

		if (request.Username != null) user.Username = request.Username;
		if (request.Email != null) user.Email = request.Email;
		if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
		if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
		if (request.TwoFactorEnabled.HasValue) user.TwoFactorEnabled = request.TwoFactorEnabled.Value;
		if (request.Metadata != null) user.Metadata = request.Metadata;

		return Task.FromResult(new UserResult
		{
			Success = true,
			User = MapToUserInfo(user)
		});
	}

	public Task<bool> DeleteUserAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		return Task.FromResult(_users.TryRemove(userId, out _));
	}

	public Task<bool> VerifyPasswordAsync(
		string userId,
		string password,
		CancellationToken cancellationToken = default)
	{
		if (!_users.TryGetValue(userId, out var user))
			return Task.FromResult(false);

		return Task.FromResult(VerifyHashedPassword(user.PasswordHash, password));
	}

	public Task<UserResult> ChangePasswordAsync(
		string userId,
		string currentPassword,
		string newPassword,
		CancellationToken cancellationToken = default)
	{
		if (!_users.TryGetValue(userId, out var user))
		{
			return Task.FromResult(new UserResult
			{
				Success = false,
				Error = new UserError { Code = "USER_NOT_FOUND", Message = "User not found" }
			});
		}

		if (!VerifyHashedPassword(user.PasswordHash, currentPassword))
		{
			return Task.FromResult(new UserResult
			{
				Success = false,
				Error = new UserError { Code = "INVALID_PASSWORD", Message = "Current password is incorrect" }
			});
		}

		user.PasswordHash = HashPassword(newPassword);

		return Task.FromResult(new UserResult { Success = true, User = MapToUserInfo(user) });
	}

	public Task<string> GeneratePasswordResetTokenAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
		// ?? ???????: ???? Token ?? Database ?? ????? ??????
		return Task.FromResult(token);
	}

	public Task<UserResult> ResetPasswordAsync(
		string userId,
		string resetToken,
		string newPassword,
		CancellationToken cancellationToken = default)
	{
		// ?? ???????: ???? ?? Token
		if (!_users.TryGetValue(userId, out var user))
		{
			return Task.FromResult(new UserResult
			{
				Success = false,
				Error = new UserError { Code = "USER_NOT_FOUND", Message = "User not found" }
			});
		}

		user.PasswordHash = HashPassword(newPassword);
		return Task.FromResult(new UserResult { Success = true, User = MapToUserInfo(user) });
	}

	public Task<string> GenerateEmailConfirmationTokenAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
		return Task.FromResult(token);
	}

	public Task<UserResult> ConfirmEmailAsync(
		string userId,
		string token,
		CancellationToken cancellationToken = default)
	{
		if (!_users.TryGetValue(userId, out var user))
		{
			return Task.FromResult(new UserResult
			{
				Success = false,
				Error = new UserError { Code = "USER_NOT_FOUND", Message = "User not found" }
			});
		}

		user.EmailVerified = true;
		return Task.FromResult(new UserResult { Success = true, User = MapToUserInfo(user) });
	}

	public Task<bool> SetEmailVerifiedAsync(
		string userId,
		bool verified,
		CancellationToken cancellationToken = default)
	{
		if (_users.TryGetValue(userId, out var user))
		{
			user.EmailVerified = verified;
			return Task.FromResult(true);
		}
		return Task.FromResult(false);
	}

	public Task<bool> SetPhoneVerifiedAsync(
		string userId,
		bool verified,
		CancellationToken cancellationToken = default)
	{
		if (_users.TryGetValue(userId, out var user))
		{
			user.PhoneNumberVerified = verified;
			return Task.FromResult(true);
		}
		return Task.FromResult(false);
	}

	public Task<bool> SetTwoFactorEnabledAsync(
		string userId,
		bool enabled,
		CancellationToken cancellationToken = default)
	{
		if (_users.TryGetValue(userId, out var user))
		{
			user.TwoFactorEnabled = enabled;
			return Task.FromResult(true);
		}
		return Task.FromResult(false);
	}

	public Task<bool> LockUserAsync(
		string userId,
		DateTimeOffset? lockoutEnd = null,
		CancellationToken cancellationToken = default)
	{
		if (_users.TryGetValue(userId, out var user))
		{
			user.IsLocked = true;
			user.LockoutEnd = lockoutEnd;
			return Task.FromResult(true);
		}
		return Task.FromResult(false);
	}

	public Task<bool> UnlockUserAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		if (_users.TryGetValue(userId, out var user))
		{
			user.IsLocked = false;
			user.LockoutEnd = null;
			return Task.FromResult(true);
		}
		return Task.FromResult(false);
	}

	private UserInfo MapToUserInfo(InMemoryUser user)
	{
		return new UserInfo
		{
			UserId = user.UserId,
			Username = user.Username,
			Email = user.Email,
			PhoneNumber = user.PhoneNumber,
			EmailVerified = user.EmailVerified,
			PhoneNumberVerified = user.PhoneNumberVerified,
			TwoFactorEnabled = user.TwoFactorEnabled,
			IsActive = user.IsActive,
			IsLocked = user.IsLocked,
			LockoutEnd = user.LockoutEnd,
			LastLoginAt = user.LastLoginAt,
			Roles = user.Roles,
			Claims = user.Claims,
			Metadata = user.Metadata
		};
	}

	private string HashPassword(string password)
	{
		return Convert.ToBase64String(KeyDerivation.Pbkdf2(
			password: password,
			salt: [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08], // ??????? ???
			prf: KeyDerivationPrf.HMACSHA256,
			iterationCount: 10000,
			numBytesRequested: 256 / 8));
	}

	private bool VerifyHashedPassword(string hashedPassword, string password)
	{
		return hashedPassword == HashPassword(password);
	}

	private class InMemoryUser
	{
		public string UserId { get; set; } = default!;
		public string Username { get; set; } = default!;
		public string Email { get; set; } = default!;
		public string? PhoneNumber { get; set; }
		public string PasswordHash { get; set; } = default!;
		public bool EmailVerified { get; set; }
		public bool PhoneNumberVerified { get; set; }
		public bool TwoFactorEnabled { get; set; }
		public bool IsActive { get; set; }
		public bool IsLocked { get; set; }
		public DateTimeOffset? LockoutEnd { get; set; }
		public DateTimeOffset? LastLoginAt { get; set; }
		public List<string> Roles { get; set; } = new();
		public Dictionary<string, string> Claims { get; set; } = new();
		public Dictionary<string, string> Metadata { get; set; } = new();
	}
}


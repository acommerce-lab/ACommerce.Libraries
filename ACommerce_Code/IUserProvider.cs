// Providers/IUserProvider.cs
using ACommerce.Authentication.Users.Abstractions.Models;

namespace ACommerce.Authentication.Users.Abstractions;

/// <summary>
/// ???? ????? ?????????? - Contract ??? ?? ????? ?? ?????
/// </summary>
public interface IUserProvider
{
	string ProviderName { get; }

	Task<UserResult> CreateUserAsync(
		CreateUserRequest request,
		CancellationToken cancellationToken = default);

	Task<UserInfo?> GetUserByIdAsync(
		string userId,
		CancellationToken cancellationToken = default);

	Task<UserInfo?> GetUserByEmailAsync(
		string email,
		CancellationToken cancellationToken = default);

	Task<UserInfo?> GetUserByUsernameAsync(
		string username,
		CancellationToken cancellationToken = default);

	Task<UserResult> UpdateUserAsync(
		string userId,
		UpdateUserRequest request,
		CancellationToken cancellationToken = default);

	Task<bool> DeleteUserAsync(
		string userId,
		CancellationToken cancellationToken = default);

	Task<bool> VerifyPasswordAsync(
		string userId,
		string password,
		CancellationToken cancellationToken = default);

	Task<UserResult> ChangePasswordAsync(
		string userId,
		string currentPassword,
		string newPassword,
		CancellationToken cancellationToken = default);

	Task<string> GeneratePasswordResetTokenAsync(
		string userId,
		CancellationToken cancellationToken = default);

	Task<UserResult> ResetPasswordAsync(
		string userId,
		string resetToken,
		string newPassword,
		CancellationToken cancellationToken = default);

	Task<string> GenerateEmailConfirmationTokenAsync(
		string userId,
		CancellationToken cancellationToken = default);

	Task<UserResult> ConfirmEmailAsync(
		string userId,
		string token,
		CancellationToken cancellationToken = default);

	Task<bool> SetEmailVerifiedAsync(
		string userId,
		bool verified,
		CancellationToken cancellationToken = default);

	Task<bool> SetPhoneVerifiedAsync(
		string userId,
		bool verified,
		CancellationToken cancellationToken = default);

	Task<bool> SetTwoFactorEnabledAsync(
		string userId,
		bool enabled,
		CancellationToken cancellationToken = default);

	Task<bool> LockUserAsync(
		string userId,
		DateTimeOffset? lockoutEnd = null,
		CancellationToken cancellationToken = default);

	Task<bool> UnlockUserAsync(
		string userId,
		CancellationToken cancellationToken = default);
}


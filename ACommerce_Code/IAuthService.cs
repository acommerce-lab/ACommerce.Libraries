using ACommerce.Auth.Core.Domain.Models;

namespace ACommerce.Auth.Core.Application.Contracts;

public interface IAuthService
{
	Task<AuthResult> LoginAsync(string username, string password);
	Task<AuthResult> RefreshTokenAsync(string refreshToken);
}


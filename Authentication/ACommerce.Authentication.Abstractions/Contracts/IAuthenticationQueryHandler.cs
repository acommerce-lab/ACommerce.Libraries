using ACommerce.Authentication.Abstractions.Queries;

namespace ACommerce.Authentication.Abstractions.Contracts;

/// <summary>
/// Handler for authentication queries
/// </summary>
public interface IAuthenticationQueryHandler
{
    Task<UserDto?> GetUserByIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<TokenValidationDto> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<UserDto?> GetUserByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<UserDto?> GetUserByPhoneAsync(
        string phoneNumber,
        CancellationToken cancellationToken = default);
}
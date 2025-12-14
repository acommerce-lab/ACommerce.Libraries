namespace ACommerce.Authentication.Abstractions.Queries;

/// <summary>
/// Query to validate JWT token and extract user ID
/// </summary>
public record ValidateTokenQuery
{
    public required string Token { get; init; }
}

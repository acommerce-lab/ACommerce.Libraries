namespace ACommerce.Authentication.Abstractions.Queries;

/// <summary>
/// Query to get user by ID
/// </summary>
public record GetUserByIdQuery
{
    public required string UserId { get; init; }
}

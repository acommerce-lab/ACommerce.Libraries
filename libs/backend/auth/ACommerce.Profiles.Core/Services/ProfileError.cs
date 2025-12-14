namespace ACommerce.Profiles.Core.Services;

/// <summary>
/// Profile error information
/// </summary>
public record ProfileError
{
    public string Code { get; init; } = default!;
    public string Message { get; init; } = default!;
    public string? Details { get; init; }
}
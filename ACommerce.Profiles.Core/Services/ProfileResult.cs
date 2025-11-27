using ACommerce.Profiles.Core.DTOs;

namespace ACommerce.Profiles.Core.Services;

/// <summary>
/// Result of profile operations
/// </summary>
public record ProfileResult
{
    public bool Success { get; init; }
    public ProfileResponseDto? Profile { get; init; }
    public ProfileError? Error { get; init; }
    public string? Message { get; init; }

    public static ProfileResult Ok(ProfileResponseDto profile, string? message = null)
        => new() { Success = true, Profile = profile, Message = message };

    public static ProfileResult Fail(ProfileError error)
        => new() { Success = false, Error = error };

    public static ProfileResult Fail(string code, string message, string? details = null)
        => new() { Success = false, Error = new ProfileError { Code = code, Message = message, Details = details } };
}

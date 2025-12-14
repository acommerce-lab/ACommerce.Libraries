using ACommerce.Profiles.Core.DTOs;

namespace ACommerce.Profiles.Core.Services;

/// <summary>
/// Service for managing user profiles
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Create a new user profile
    /// </summary>
    Task<ProfileResult> CreateProfileAsync(
        CreateProfileDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get profile by user ID
    /// </summary>
    Task<ProfileResponseDto?> GetProfileByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get profile by ID
    /// </summary>
    Task<ProfileResponseDto?> GetProfileByIdAsync(
        Guid profileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get profile by chat identifier
    /// </summary>
    Task<ProfileResponseDto?> GetProfileByChatIdentifierAsync(
        string chatIdentifier,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update profile
    /// </summary>
    Task<ProfileResult> UpdateProfileAsync(
        string userId,
        UpdateProfileDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add contact point to profile
    /// </summary>
    Task<ProfileResult> AddContactPointAsync(
        string userId,
        CreateContactPointDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify contact point
    /// </summary>
    Task<ProfileResult> VerifyContactPointAsync(
        string userId,
        ContactPointType type,
        string value,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove contact point
    /// </summary>
    Task<ProfileResult> RemoveContactPointAsync(
        string userId,
        Guid contactPointId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Set primary contact point for a type
    /// </summary>
    Task<ProfileResult> SetPrimaryContactPointAsync(
        string userId,
        Guid contactPointId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all contact points for a user
    /// </summary>
    Task<List<ContactPointDto>> GetContactPointsAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Search profiles by display name
    /// </summary>
    Task<List<ProfileResponseDto>> SearchProfilesAsync(
        string searchTerm,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}

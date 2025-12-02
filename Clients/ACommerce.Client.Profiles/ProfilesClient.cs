using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Profiles;

public sealed class ProfilesClient(IApiClient httpClient)
{
    private const string ServiceName = "Marketplace";

    public async Task<ProfileDto?> GetMyProfileAsync(CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<ProfileDto>(ServiceName, "/api/profiles/me", cancellationToken);
	}

	public async Task<ProfileDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await httpClient.GetAsync<ProfileDto>(ServiceName, $"/api/profiles/{id}", cancellationToken);
	}

	public async Task<ProfileDto?> CreateAsync(CreateProfileRequest request, CancellationToken cancellationToken = default)
	{
		return await httpClient.PostAsync<CreateProfileRequest, ProfileDto>(ServiceName, "/api/profiles", request, cancellationToken);
	}

	public async Task<ProfileDto?> UpdateAsync(Guid id, UpdateProfileRequest request, CancellationToken cancellationToken = default)
	{
		return await httpClient.PutAsync<UpdateProfileRequest, ProfileDto>(ServiceName, $"/api/profiles/{id}", request, cancellationToken);
	}

	/// <summary>
	/// تحديث بروفايلي
	/// </summary>
	public async Task<ProfileDto?> UpdateMyProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
	{
		return await httpClient.PutAsync<UpdateProfileRequest, ProfileDto>(ServiceName, "/api/profiles/me", request, cancellationToken);
	}
}

/// <summary>
/// Profile DTO for client-side use
/// </summary>
public sealed class ProfileDto
{
	public Guid Id { get; set; }
	public string UserId { get; set; } = string.Empty;
	public string? FullName { get; set; }
	public string? BusinessName { get; set; }
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Address { get; set; }
	public string? City { get; set; }
	public string? Country { get; set; }
	public string? Avatar { get; set; }
	public bool IsActive { get; set; }
	public bool IsVerified { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

public sealed class CreateProfileRequest
{
	public string UserId { get; set; } = string.Empty;
	public string? FullName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Address { get; set; }
}

public sealed class UpdateProfileRequest
{
	public string? FullName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public string? Address { get; set; }
	public string? City { get; set; }
	public string? Avatar { get; set; }
}

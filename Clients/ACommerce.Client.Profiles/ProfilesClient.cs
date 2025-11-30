using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Profiles;

public sealed class ProfilesClient
{
	private readonly IApiClient _httpClient;
	private const string ServiceName = "Marketplace";

	public ProfilesClient(IApiClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<ProfileDto?> GetMyProfileAsync(CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<ProfileDto>(ServiceName, "/api/profiles/me", cancellationToken);
	}

	public async Task<ProfileDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<ProfileDto>(ServiceName, $"/api/profiles/{id}", cancellationToken);
	}

	public async Task<ProfileDto?> CreateAsync(CreateProfileRequest request, CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<CreateProfileRequest, ProfileDto>(ServiceName, "/api/profiles", request, cancellationToken);
	}

	public async Task<ProfileDto?> UpdateAsync(Guid id, UpdateProfileRequest request, CancellationToken cancellationToken = default)
	{
		return await _httpClient.PutAsync<UpdateProfileRequest, ProfileDto>(ServiceName, $"/api/profiles/{id}", request, cancellationToken);
	}

	/// <summary>
	/// تحديث بروفايلي
	/// </summary>
	public async Task<ProfileDto?> UpdateMyProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
	{
		return await _httpClient.PutAsync<UpdateProfileRequest, ProfileDto>(ServiceName, "/api/profiles/me", request, cancellationToken);
	}
}

/// <summary>
/// Profile DTO for client-side use
/// </summary>
public sealed class ProfileDto
{
	public Guid Id { get; set; }
	public string UserId { get; set; } = string.Empty;
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string? DisplayName { get; set; }
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Address { get; set; }
	public string? AvatarUrl { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}

public sealed class CreateProfileRequest
{
	public string UserId { get; set; } = string.Empty;
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string? PhoneNumber { get; set; }
	public string? Address { get; set; }
}

public sealed class UpdateProfileRequest
{
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Address { get; set; }
}

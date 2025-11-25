using ACommerce.Client.Core.Http;
using ACommerce.Profiles.Entities;

namespace ACommerce.Client.Profiles;

public sealed class ProfilesClient
{
	private readonly DynamicHttpClient _httpClient;
	private const string ServiceName = "Marketplace";

	public ProfilesClient(DynamicHttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task<Profile?> GetMyProfileAsync(CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<Profile>(ServiceName, "/api/profiles/me", cancellationToken);
	}

	public async Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await _httpClient.GetAsync<Profile>(ServiceName, $"/api/profiles/{id}", cancellationToken);
	}

	public async Task<Profile?> CreateAsync(CreateProfileRequest request, CancellationToken cancellationToken = default)
	{
		return await _httpClient.PostAsync<CreateProfileRequest, Profile>(ServiceName, "/api/profiles", request, cancellationToken);
	}

	public async Task<Profile?> UpdateAsync(Guid id, UpdateProfileRequest request, CancellationToken cancellationToken = default)
	{
		return await _httpClient.PutAsync<UpdateProfileRequest, Profile>(ServiceName, $"/api/profiles/{id}", request, cancellationToken);
	}
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

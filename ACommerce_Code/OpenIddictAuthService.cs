using ACommerce.Auth.Core.Application.Contracts;
using ACommerce.Auth.Core.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace ACommerce.Auth.OpenIddict;

public class OpenIddictAuthService(IHttpClientFactory clientFactory, IConfiguration config) : IAuthService
{
	public async Task<AuthResult> LoginAsync(string username, string password)
	{
		var client = clientFactory.CreateClient("OpenIddictAuth");
		var tokenEndpoint = config["Auth:OpenIddict:TokenEndpoint"] ?? "/api/connect/token";

		var content = new FormUrlEncodedContent(new Dictionary<string, string>
		{
			["grant_type"] = "password",
			["username"] = username,
			["password"] = password,
			["scope"] = "openid profile email offline_access"
		});

		var response = await client.PostAsync(tokenEndpoint, content);

		if (!response.IsSuccessStatusCode)
		{
			return new AuthResult { Success = false, Error = "??? ?????? ??? ?????? ?? OpenIddict" };
		}

		var tokenData = await response.Content.ReadFromJsonAsync<JsonElement>();
		return new AuthResult
		{
			Success = true,
			Token = tokenData.GetProperty("access_token").GetString(),
			RefreshToken = tokenData.TryGetProperty("refresh_token", out var r)
				? r.GetString()
				: null
		};
	}

	public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
	{
		var client = clientFactory.CreateClient("OpenIddictAuth");
		var tokenEndpoint = config["Auth:OpenIddict:TokenEndpoint"] ?? "/api/connect/token";

		var content = new FormUrlEncodedContent(new Dictionary<string, string>
		{
			["grant_type"] = "refresh_token",
			["refresh_token"] = refreshToken
		});

		var response = await client.PostAsync(tokenEndpoint, content);

		if (!response.IsSuccessStatusCode)
			return new AuthResult { Success = false, Error = "??? ????? ??????" };

		var tokenData = await response.Content.ReadFromJsonAsync<JsonElement>();
		return new AuthResult
		{
			Success = true,
			Token = tokenData.GetProperty("access_token").GetString(),
			RefreshToken = tokenData.GetProperty("refresh_token").GetString()
		};
	}
}


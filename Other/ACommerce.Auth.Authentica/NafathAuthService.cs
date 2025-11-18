using ACommerce.Auth.Core.Application.Contracts;
using ACommerce.Auth.Core.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace ACommerce.Auth.Authentica;

public class NafathAuthService(
	IHttpClientFactory clientFactory,
	IConfiguration config)
	: IAuthService
{
	public async Task<AuthResult> LoginAsync(string username, string password)
	{
		var client = clientFactory.CreateClient("NafathAuth");
		client.BaseAddress = new Uri(config["Auth:Nafath:Authority"]!);

		var response = await client.PostAsJsonAsync("/api/nafath/authenticate", new
		{
			username,
			password
		});

		if (!response.IsSuccessStatusCode)
			return new AuthResult { Success = false, Error = "??? ????? ?????? ??? ????" };

		var data = await response.Content.ReadFromJsonAsync<AuthResult>();
		return data ?? new AuthResult { Success = false, Error = "??????? ??? ?????? ?? ????" };
	}

	public Task<AuthResult> RefreshTokenAsync(string refreshToken)
		=> Task.FromResult(new AuthResult { Success = false, Error = "??? ????? ?????? ?? ????" });
}


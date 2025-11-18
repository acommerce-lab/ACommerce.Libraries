using ACommerce.Auth.Core.Application.Contracts;
using ACommerce.Auth.Core.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

namespace ACommerce.Auth.JWT;

public class JwtAuthService(IConfiguration config) : IAuthService
{
	public Task<AuthResult> LoginAsync(string username, string password)
	{
		// ?? ?? ???? ??????? ???? ?? ?????? ???????? ????? ??? UserManager
		if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			return Task.FromResult(new AuthResult { Success = false, Error = "?????? ??? ?????" });

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, username),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new Claim("aud", config["Jwt:Audience"]!)
		};

		var token = new JwtSecurityToken(
			issuer: config["Jwt:Issuer"],
			audience: config["Jwt:Audience"],
			claims: claims,
			expires: DateTime.UtcNow.AddHours(1),
			signingCredentials: creds);

		return Task.FromResult(new AuthResult
		{
			Success = true,
			Token = new JwtSecurityTokenHandler().WriteToken(token)
		});
	}

	public Task<AuthResult> RefreshTokenAsync(string refreshToken)
		=> Task.FromResult(new AuthResult { Success = false, Error = "JWT ?????? ?? ???? ???????" });
}


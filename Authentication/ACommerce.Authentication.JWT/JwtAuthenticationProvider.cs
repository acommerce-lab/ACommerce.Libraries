using ACommerce.Authentication.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TokenValidationResult = ACommerce.Authentication.Abstractions.TokenValidationResult;

namespace ACommerce.Authentication.JWT;

public class JwtAuthenticationProvider : IAuthenticationProvider
{
	private readonly JwtOptions _options;
	private readonly ILogger<JwtAuthenticationProvider> _logger;
	private readonly JwtSecurityTokenHandler _tokenHandler;
	private readonly SigningCredentials _signingCredentials;
	private readonly TokenValidationParameters _validationParameters;

	public string ProviderName => "JWT";

	public JwtAuthenticationProvider(
		IOptions<JwtOptions> options,
		ILogger<JwtAuthenticationProvider> logger)
	{
		_options = options.Value ?? throw new ArgumentNullException(nameof(options));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));

		ValidateOptions();

		_tokenHandler = new JwtSecurityTokenHandler();

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
		_signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		_validationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = key,
			ValidateIssuer = true,
			ValidIssuer = _options.Issuer,
			ValidateAudience = true,
			ValidAudience = _options.Audience,
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero
		};
	}

	public Task<AuthenticationResult> AuthenticateAsync(
		AuthenticationRequest request,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var claims = new List<Claim>
			{
				new(JwtRegisteredClaimNames.Sub, request.Identifier),
				new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new(JwtRegisteredClaimNames.Iat,
					DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
					ClaimValueTypes.Integer64)
			};

			if (request.Claims != null)
			{
				foreach (var claim in request.Claims)
				{
					claims.Add(new Claim(claim.Key, claim.Value));
				}
			}

			var expiresAt = DateTimeOffset.UtcNow.Add(_options.AccessTokenLifetime);

			var token = new JwtSecurityToken(
				issuer: _options.Issuer,
				audience: _options.Audience,
				claims: claims,
				expires: expiresAt.UtcDateTime,
				signingCredentials: _signingCredentials
			);

			var tokenString = _tokenHandler.WriteToken(token);

			_logger.LogInformation(
				"JWT token generated successfully for user {Identifier}",
				request.Identifier);

			return Task.FromResult(new AuthenticationResult
			{
				Success = true,
				AccessToken = tokenString,
				TokenType = "Bearer",
				ExpiresAt = expiresAt,
				UserId = request.Identifier
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to generate JWT token");

			return Task.FromResult(new AuthenticationResult
			{
				Success = false,
				Error = new AuthenticationError
				{
					Code = "JWT_GENERATION_FAILED",
					Message = "Failed to generate authentication token",
					Details = ex.Message
				}
			});
		}
	}

	public Task<AuthenticationResult> RefreshAsync(
		string refreshToken,
		CancellationToken cancellationToken = default)
	{
		return Task.FromResult(new AuthenticationResult
		{
			Success = false,
			Error = new AuthenticationError
			{
				Code = "NOT_SUPPORTED",
				Message = "JWT provider does not support token refresh. Consider using OpenIddict for refresh token support."
			}
		});
	}

	public Task<TokenValidationResult> ValidateTokenAsync(
		string token,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var principal = _tokenHandler.ValidateToken(
				token,
				_validationParameters,
				out var validatedToken);

			var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

			var claims = principal.Claims
				.ToDictionary(c => c.Type, c => c.Value);

			return Task.FromResult(new TokenValidationResult
			{
				IsValid = true,
				UserId = userId,
				Claims = claims
			});
		}
		catch (SecurityTokenException ex)
		{
			_logger.LogWarning(ex, "Token validation failed");

			return Task.FromResult(new TokenValidationResult
			{
				IsValid = false,
				Error = ex.Message
			});
		}
	}

	public Task<bool> RevokeTokenAsync(
		string token,
		CancellationToken cancellationToken = default)
	{
		_logger.LogWarning(
			"JWT tokens are stateless and cannot be revoked. " +
			"Consider using a token blacklist or switch to OpenIddict.");

		return Task.FromResult(false);
	}

	private void ValidateOptions()
	{
		if (string.IsNullOrWhiteSpace(_options.SecretKey))
			throw new InvalidOperationException("JWT SecretKey is required");

		if (_options.SecretKey.Length < 32)
			throw new InvalidOperationException("JWT SecretKey must be at least 32 characters");

		if (string.IsNullOrWhiteSpace(_options.Issuer))
			throw new InvalidOperationException("JWT Issuer is required");

		if (string.IsNullOrWhiteSpace(_options.Audience))
			throw new InvalidOperationException("JWT Audience is required");
	}
}

public class JwtOptions
{
	public const string SectionName = "Authentication:JWT";

	[Required(ErrorMessage = "JWT SecretKey is required")]
	[MinLength(32, ErrorMessage = "JWT SecretKey must be at least 32 characters")]
	public string SecretKey { get; set; } = default!;

	[Required(ErrorMessage = "JWT Issuer is required")]
	public string Issuer { get; set; } = default!;

	[Required(ErrorMessage = "JWT Audience is required")]
	public string Audience { get; set; } = default!;

	public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromHours(1);
}

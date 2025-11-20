namespace ACommerce.Auth.OpenIddict.Domain.DTOs;

public class OpenIddictOptions
{
	public string? Issuer { get; set; }
	public string? EncryptionKey { get; set; }
	public string? SigningKey { get; set; }
	public string? TokenEndpoint { get; set; }
	public string? UserInfoEndpoint { get; set; }
}


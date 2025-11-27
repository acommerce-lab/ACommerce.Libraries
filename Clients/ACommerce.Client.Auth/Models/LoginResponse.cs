namespace ACommerce.Client.Auth.Models;

public sealed class LoginResponse
{
	public string Token { get; set; } = string.Empty;
	public string Username { get; set; } = string.Empty;
	public string? Email { get; set; }
	public string Role { get; set; } = string.Empty;
	public DateTime ExpiresAt { get; set; }
	public bool RequiresTwoFactor { get; set; }
	public string? TwoFactorMethod { get; set; }
	public string? SessionId { get; set; }
}

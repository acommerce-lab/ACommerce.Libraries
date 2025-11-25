namespace ACommerce.Client.Auth.Models;

public sealed class RegisterRequest
{
	public string Username { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
	public string Role { get; set; } = "Customer";
}

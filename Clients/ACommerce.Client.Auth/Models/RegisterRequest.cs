namespace ACommerce.Client.Auth.Models;

public sealed class RegisterRequest
{
	public string Username { get; set; } = string.Empty;
	public string FullName { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string PhoneNumber { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
	public string? ConfirmPassword { get; set; }
	public string Role { get; set; } = "Customer";
	public bool AcceptTerms { get; set; }
}

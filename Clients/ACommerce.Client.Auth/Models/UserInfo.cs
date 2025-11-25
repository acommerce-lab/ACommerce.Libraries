namespace ACommerce.Client.Auth.Models;

public sealed class UserInfo
{
	public string UserId { get; set; } = string.Empty;
	public string Username { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Role { get; set; } = string.Empty;
}

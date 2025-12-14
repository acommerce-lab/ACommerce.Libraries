namespace ACommerce.Authentication.Abstractions;

public record AuthenticationRequest
{
	public required string Identifier { get; init; }
	public string? Credential { get; init; }
	public Dictionary<string, string>? Claims { get; init; }
	public Dictionary<string, object>? Metadata { get; init; }
}


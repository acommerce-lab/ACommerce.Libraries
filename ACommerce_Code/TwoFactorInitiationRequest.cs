namespace ACommerce.Authentication.Abstractions;

public record TwoFactorInitiationRequest
{
	public required string Identifier { get; init; }
	public Dictionary<string, string>? Metadata { get; init; }
}


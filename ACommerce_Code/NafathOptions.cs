using System.ComponentModel.DataAnnotations;

namespace ACommerce.Authentication.TwoFactor.Nafath;

public class NafathOptions
{
	public const string SectionName = "Authentication:TwoFactor:Nafath";
	public const string HttpClientName = "Nafath";

	[Required(ErrorMessage = "Nafath BaseUrl is required")]
	[Url(ErrorMessage = "Nafath BaseUrl must be a valid URL")]
	public string BaseUrl { get; set; } = "https://api.authentica.sa/api/v2/";

	/// <summary>
	/// API Key for Nafath service (X-Authorization header)
	/// </summary>
	public string? ApiKey { get; set; }

	/// <summary>
	/// Secret for validating webhook callbacks
	/// </summary>
	public string? WebhookSecret { get; set; }

	public NafathMode Mode { get; set; } = NafathMode.Production;

	public string TestNationalId { get; set; } = "2507643761";

	[Range(1, 60, ErrorMessage = "Session expiration must be between 1 and 60 minutes")]
	public int SessionExpirationMinutes { get; set; } = 5;
}


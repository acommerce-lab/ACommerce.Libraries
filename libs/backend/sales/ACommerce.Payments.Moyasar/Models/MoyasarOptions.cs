namespace ACommerce.Payments.Moyasar.Models;

/// <summary>
/// خيارات Moyasar
/// </summary>
public class MoyasarOptions
{
	public required string ApiKey { get; set; }
	public required string PublishableKey { get; set; }
	public string ApiUrl { get; set; } = "https://api.moyasar.com/v1";
	public bool UseSandbox { get; set; }
}

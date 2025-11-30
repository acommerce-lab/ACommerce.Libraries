using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Client.Realtime.Extensions;

/// <summary>
/// Options لتكوين Realtime Client
/// </summary>
public class RealtimeClientOptions
{
	public string HubUrl { get; set; } = "";
	public bool AutoReconnect { get; set; } = true;
}

/// <summary>
/// Extensions لتسجيل Realtime Client
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// إضافة Realtime Client
	/// </summary>
	public static IServiceCollection AddRealtimeClient(
		this IServiceCollection services,
		Action<RealtimeClientOptions> configure)
	{
		var options = new RealtimeClientOptions();
		configure(options);

		// Register options
		services.AddSingleton(options);

		// Register RealtimeClient as singleton to maintain connection
		services.AddSingleton<RealtimeClient>();
		services.AddSingleton<IRealtimeClient>(sp => sp.GetRequiredService<RealtimeClient>());

		return services;
	}
}

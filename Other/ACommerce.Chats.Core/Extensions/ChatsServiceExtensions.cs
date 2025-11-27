using ACommerce.Chats.Abstractions.Providers;
using ACommerce.Chats.Core.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Chats.Core.Extensions;

/// <summary>
/// امتدادات لتسجيل خدمات الدردشة
/// </summary>
public static class ChatsServiceExtensions
{
	/// <summary>
	/// إضافة خدمات الدردشة
	/// </summary>
	public static IServiceCollection AddChatsCore(this IServiceCollection services)
	{
		// مزودات الدردشة
		services.AddScoped<IChatProvider, DatabaseChatProvider>();
		services.AddScoped<IMessageProvider, DatabaseMessageProvider>();
		services.AddScoped<IRealtimeChatProvider, SignalRRealtimeChatProvider>();

		// مزود الحضور (Singleton لأنه يحتفظ بالحالة في الذاكرة)
		services.AddSingleton<IPresenceProvider, InMemoryPresenceProvider>();

		return services;
	}
}

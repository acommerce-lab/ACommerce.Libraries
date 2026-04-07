using ACommerce.ServiceMessaging.Operations.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.ServiceMessaging.InMemory.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// يسجل InMemoryServiceMessageBus كمزود لـ IMessageBus.
    /// مناسب للتطوير والاختبار والنشر الأحادي.
    ///
    /// الاستخدام:
    ///   services.AddInMemoryServiceMessaging();
    /// </summary>
    public static IServiceCollection AddInMemoryServiceMessaging(this IServiceCollection services)
    {
        // Singleton مشترك بين كل الخدمات في نفس العملية
        services.AddSingleton<InMemoryServiceMessageBus>();
        services.AddSingleton<IMessageBus>(sp => sp.GetRequiredService<InMemoryServiceMessageBus>());

        return services;
    }
}

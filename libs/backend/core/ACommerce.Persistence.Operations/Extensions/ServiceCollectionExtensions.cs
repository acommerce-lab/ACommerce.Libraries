using Microsoft.Extensions.DependencyInjection;

namespace ACommerce.Persistence.Operations.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// يسجّل RepositoryPersistenceInterceptor كخدمة Scoped جاهزة للحقن في المعترضات.
    ///
    /// ملاحظة: المعترض نفسه يحتاج تسجيله في OperationInterceptorRegistry - إما يدوياً
    /// أو عبر PredicateInterceptor يحلّ المعترض لكل عملية عبر IServiceProvider.
    /// </summary>
    public static IServiceCollection AddPersistenceInterceptor(this IServiceCollection services)
    {
        services.AddScoped<RepositoryPersistenceInterceptor>();
        return services;
    }
}

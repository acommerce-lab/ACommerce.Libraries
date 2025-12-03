using ACommerce.Client.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace Ashare.Web.Services;

/// <summary>
/// Wrapper لـ TokenManager يمكن استخدامه كـ singleton
/// يستخدم IServiceScopeFactory للوصول إلى TokenManager الـ scoped
/// </summary>
public class ScopedTokenProvider : ITokenProvider
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ScopedTokenProvider(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<string?> GetTokenAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var tokenManager = scope.ServiceProvider.GetRequiredService<ACommerce.Client.Auth.TokenManager>();
        return await tokenManager.GetTokenAsync();
    }
}

using ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ACommerce.Authentication.TwoFactor.SessionStore.EntityFramework.Extensions;

public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configures two-factor session entity in your DbContext
    /// </summary>
    public static ModelBuilder ApplyTwoFactorSessionConfiguration(
        this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new TwoFactorSessionConfiguration());
        return modelBuilder;
    }
}
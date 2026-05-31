namespace ACommerce.Client.AppConfig.Services;

/// <summary>
/// واجهة موجزة لفحص Feature Flags من شيفرة UI / Razor.
/// مثال: @if (Features.IsEnabled("booking.enabled")) { ... }
/// </summary>
public interface IFeatureFlags
{
    bool IsEnabled(string key, bool fallback = false);
}

internal sealed class StoreBackedFeatureFlags(AppConfigStore store) : IFeatureFlags
{
    public bool IsEnabled(string key, bool fallback = false) => store.IsFeatureEnabled(key, fallback);
}

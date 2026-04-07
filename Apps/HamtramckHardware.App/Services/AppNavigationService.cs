using ACommerce.Templates.Customer.Services;
using Microsoft.AspNetCore.Components;

namespace HamtramckHardware.App.Services;

/// <summary>
/// MAUI implementation of navigation service
/// </summary>
public class AppNavigationService : BaseNavigationService
{
    public AppNavigationService(NavigationManager navigationManager)
        : base(navigationManager)
    {
    }

    public override bool IsMobileApp => true;

    public override Task OpenExternalAsync(string url) => null;

    public override Task OpenMapAsync(double latitude, double longitude, string? label = null) => null;
}

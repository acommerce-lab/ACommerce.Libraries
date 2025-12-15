using Microsoft.AspNetCore.Components;
using ACommerce.Templates.Customer.Services;

namespace ACommerce.App.Customer.Services;

/// <summary>
/// Navigation service for the customer MAUI app
/// </summary>
public class AppNavigationService : IAppNavigationService
{
    private readonly NavigationManager _navigationManager;

    public AppNavigationService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public string CurrentUri => _navigationManager.Uri;

    public void NavigateTo(string uri, bool forceLoad = false)
    {
        _navigationManager.NavigateTo(uri, forceLoad);
    }

    public void NavigateBack()
    {
        // In MAUI Blazor Hybrid, we use Shell navigation for back
        Shell.Current?.GoToAsync("..");
    }

    public void NavigateToAndClearHistory(string uri)
    {
        // Navigate and prevent back navigation
        _navigationManager.NavigateTo(uri, forceLoad: true, replace: true);
    }

    public async Task OpenMapAsync(double latitude, double longitude, string? label = null)
    {
        try
        {
            var location = new Location(latitude, longitude);
            var options = new MapLaunchOptions
            {
                Name = label
            };
            await Map.Default.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening map: {ex.Message}");
        }
    }

    public async Task OpenExternalAsync(string url)
    {
        try
        {
            await Browser.Default.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error opening external URL: {ex.Message}");
        }
    }
}

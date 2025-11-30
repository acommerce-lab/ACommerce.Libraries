using Microsoft.AspNetCore.Components;

namespace ACommerce.App.Customer.Services;

/// <summary>
/// Navigation service for the customer app
/// </summary>
public class AppNavigationService : IAppNavigationService
{
    private readonly NavigationManager _navigationManager;

    public AppNavigationService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public string CurrentUrl => _navigationManager.Uri;

    public void NavigateTo(string url)
    {
        _navigationManager.NavigateTo(url);
    }

    public void NavigateBack()
    {
        // In MAUI, we need to use JavaScript interop for back navigation
        // For now, navigate to home
        _navigationManager.NavigateTo("/");
    }
}

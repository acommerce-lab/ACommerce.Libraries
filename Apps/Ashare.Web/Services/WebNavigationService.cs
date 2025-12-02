using Ashare.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace Ashare.Web;

/// <summary>
/// Web implementation of navigation service
/// </summary>
public class WebNavigationService : IAppNavigationService
{
    private readonly NavigationManager _navigationManager;
    private readonly Stack<string> _history = new();

    public WebNavigationService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public void NavigateTo(string uri, bool forceLoad = false)
    {
        _history.Push(_navigationManager.Uri);
        _navigationManager.NavigateTo(uri, forceLoad);
    }

    public void NavigateBack()
    {
        if (_history.Count > 0)
        {
            var previousUri = _history.Pop();
            _navigationManager.NavigateTo(previousUri);
        }
        else
        {
            _navigationManager.NavigateTo("/");
        }
    }

    public string CurrentUri => _navigationManager.Uri;
}

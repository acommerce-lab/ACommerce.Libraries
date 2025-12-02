using Microsoft.AspNetCore.Components;

namespace Ashare.Shared.Services;

/// <summary>
/// تنفيذ خدمة التنقل مع دعم السجل
/// </summary>
public class AppNavigationService : IAppNavigationService
{
    private readonly NavigationManager _navigationManager;
    private readonly Stack<string> _history = new();

    public AppNavigationService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public string CurrentUrl => _navigationManager.Uri;

    public void NavigateTo(string url)
    {
        _history.Push(_navigationManager.Uri);
        _navigationManager.NavigateTo(url);
    }

    public void NavigateBack()
    {
        if (_history.Count > 0)
        {
            var previousUrl = _history.Pop();
            _navigationManager.NavigateTo(previousUrl);
        }
        else
        {
            _navigationManager.NavigateTo("/");
        }
    }
}

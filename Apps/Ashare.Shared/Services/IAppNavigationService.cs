namespace Ashare.Shared.Services;

/// <summary>
/// Navigation service interface for both MAUI and Web platforms
/// </summary>
public interface IAppNavigationService
{
    /// <summary>
    /// Navigate to the specified URI
    /// </summary>
    void NavigateTo(string uri, bool forceLoad = false);

    /// <summary>
    /// Navigate back to the previous page
    /// </summary>
    void NavigateBack();

    /// <summary>
    /// Get the current URI
    /// </summary>
    string CurrentUri { get; }
}

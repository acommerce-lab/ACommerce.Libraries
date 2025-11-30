namespace Ashare.App.Services;

/// <summary>
/// خدمة التنقل في التطبيق
/// </summary>
public interface IAppNavigationService
{
    void NavigateTo(string url);
    void NavigateBack();
    string CurrentUrl { get; }
}

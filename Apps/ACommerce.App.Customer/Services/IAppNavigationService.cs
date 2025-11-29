namespace ACommerce.App.Customer.Services;

/// <summary>
/// Navigation service interface for the customer app
/// </summary>
public interface IAppNavigationService
{
    void NavigateTo(string url);
    void NavigateBack();
    string CurrentUrl { get; }
}

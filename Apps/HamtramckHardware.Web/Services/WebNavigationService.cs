using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ACommerce.Templates.Customer.Services;

namespace HamtramckHardware.Web.Services;

/// <summary>
/// Web implementation of navigation service
/// </summary>
public class WebNavigationService : BaseNavigationService
{
    private readonly IJSRuntime _jsRuntime;

    public WebNavigationService(NavigationManager navigationManager, IJSRuntime jsRuntime)
        : base(navigationManager)
    {
        _jsRuntime = jsRuntime;
    }

    public override async Task OpenMapAsync(double latitude, double longitude, string? label = null)
    {
        var url = $"https://www.google.com/maps?q={latitude},{longitude}";
        await _jsRuntime.InvokeVoidAsync("window.open", url, "_blank");
    }

    public override async Task OpenExternalAsync(string url)
    {
        await _jsRuntime.InvokeVoidAsync("window.open", url, "_blank");
    }
}

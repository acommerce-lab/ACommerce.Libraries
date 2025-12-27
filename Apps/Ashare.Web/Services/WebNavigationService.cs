using ACommerce.Templates.Customer.Services;
using Ashare.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Ashare.Web.Services;

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

    /// <summary>
    /// Open location in maps (web version)
    /// </summary>
    public override async Task OpenMapAsync(double latitude, double longitude, string? label = null)
    {
        try
        {
            // Use the JavaScript function
            await _jsRuntime.InvokeVoidAsync("AcMap.openNativeMaps", latitude, longitude, label ?? "الموقع");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebNavigationService] Error opening map: {ex.Message}");

            // Fallback: open Google Maps in new tab
            var url = $"https://www.google.com/maps?q={latitude},{longitude}";
            await _jsRuntime.InvokeVoidAsync("window.open", url, "_blank");
        }
    }

    /// <summary>
    /// Open external URL
    /// </summary>
    public override async Task OpenExternalAsync(string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url)) return;
            await _jsRuntime.InvokeVoidAsync("window.open", url, "_blank");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebNavigationService] Error opening external URL: {ex.Message}");
        }
    }
}

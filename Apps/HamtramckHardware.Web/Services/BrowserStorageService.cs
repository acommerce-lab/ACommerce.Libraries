using ACommerce.Client.Core.Storage;
using Microsoft.JSInterop;

namespace HamtramckHardware.Web.Services;

/// <summary>
/// Browser localStorage implementation for Web
/// </summary>
public class BrowserStorageService : IStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string?> GetAsync(string key)
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
        }
        catch (InvalidOperationException) // During prerendering
        {
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserStorageService] GetAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task SetAsync(string key, string value)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        catch (InvalidOperationException) // During prerendering
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserStorageService] SetAsync error: {ex.Message}");
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch (InvalidOperationException) // During prerendering
        {
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserStorageService] RemoveAsync error: {ex.Message}");
        }
    }

    public async Task<bool> ContainsKeyAsync(string key)
    {
        var value = await GetAsync(key);
        return value != null;
    }
}

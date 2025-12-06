using Ashare.Shared.Services;
using Microsoft.JSInterop;

namespace Ashare.Web.Services;

/// <summary>
/// Browser localStorage implementation for Web
/// </summary>
public class BrowserStorageService : IStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public BrowserStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        Console.WriteLine("[BrowserStorageService] Created");
    }

    public async Task<string?> GetAsync(string key)
    {
        try
        {
            var value = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            Console.WriteLine($"[BrowserStorageService] GetAsync({key}): {(value != null ? "found" : "not found")}");
            return value;
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("prerender"))
        {
            // During prerendering, JSInterop is not available
            Console.WriteLine($"[BrowserStorageService] GetAsync({key}): prerendering, returning null");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserStorageService] GetAsync({key}) error: {ex.Message}");
            return null;
        }
    }

    public async Task SetAsync(string key, string value)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
            Console.WriteLine($"[BrowserStorageService] SetAsync({key}): success");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("prerender"))
        {
            Console.WriteLine($"[BrowserStorageService] SetAsync({key}): prerendering, skipped");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserStorageService] SetAsync({key}) error: {ex.Message}");
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            Console.WriteLine($"[BrowserStorageService] RemoveAsync({key}): success");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("prerender"))
        {
            Console.WriteLine($"[BrowserStorageService] RemoveAsync({key}): prerendering, skipped");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BrowserStorageService] RemoveAsync({key}) error: {ex.Message}");
        }
    }
}

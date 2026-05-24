using ACommerce.Templates.Customer.Services;
using Microsoft.JSInterop;

namespace Nadena.Web.Services;

/// <summary>
/// Web implementation of IMediaPickerService using a hidden &lt;input type="file"&gt;
/// driven via JS interop (window.nadenaMediaPicker.pick). Returns image bytes in-memory.
/// </summary>
public class WebMediaPickerService : IMediaPickerService
{
    private readonly IJSRuntime _js;

    public WebMediaPickerService(IJSRuntime js) => _js = js;

    // Browser file dialog handles camera via the OS picker; no separate camera API.
    public bool IsCameraAvailable => false;

    public Task<MediaPickResult?> CapturePhotoAsync() => PickSingleAsync(capture: true);

    public Task<MediaPickResult?> PickPhotoAsync() => PickSingleAsync(capture: false);

    public async Task<List<MediaPickResult>> PickPhotosAsync(int maxCount = 10)
    {
        var picked = await PickAsync(multiple: true, capture: false);
        return picked.Take(maxCount).ToList();
    }

    public Task<List<MediaPickResult>> PickOrCapturePhotosAsync(int maxCount = 10)
        => PickPhotosAsync(maxCount);

    private async Task<MediaPickResult?> PickSingleAsync(bool capture)
    {
        var list = await PickAsync(multiple: false, capture: capture);
        return list.FirstOrDefault();
    }

    private async Task<List<MediaPickResult>> PickAsync(bool multiple, bool capture)
    {
        var results = new List<MediaPickResult>();
        try
        {
            var files = await _js.InvokeAsync<PickedFile[]?>("nadenaMediaPicker.pick", "image/*", multiple, capture);
            if (files == null) return results;

            foreach (var f in files)
            {
                if (string.IsNullOrEmpty(f.Base64)) continue;
                var bytes = Convert.FromBase64String(f.Base64);
                var name = string.IsNullOrEmpty(f.FileName) ? "image.jpg" : f.FileName;
                results.Add(new MediaPickResult
                {
                    FileName = name,
                    FilePath = name,
                    ContentType = string.IsNullOrEmpty(f.ContentType) ? "image/jpeg" : f.ContentType,
                    OpenReadAsync = () => Task.FromResult<Stream>(new MemoryStream(bytes))
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WebMediaPickerService] pick error: {ex.Message}");
        }
        return results;
    }

    private sealed class PickedFile
    {
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public string? Base64 { get; set; }
    }
}

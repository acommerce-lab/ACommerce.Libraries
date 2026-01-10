namespace ACommerce.Templates.Customer.Services;

/// <summary>
/// تنفيذ افتراضي لضغط الصور - لا يقوم بضغط فعلي
/// يُستخدم على المنصات التي لا تدعم SkiaSharp (مثل Blazor Server/WASM بدون MAUI)
/// </summary>
public class DefaultImageCompressionService : IImageCompressionService
{
    public Task<CompressedImageResult> CompressAsync(
        Stream imageStream,
        int maxWidth = 1920,
        int maxHeight = 1920,
        int quality = 80)
    {
        // لا يتم الضغط - يُعيد الصورة كما هي
        Console.WriteLine("[DefaultImageCompression] No compression available on this platform");

        long size = 0;
        if (imageStream.CanSeek)
        {
            size = imageStream.Length;
            imageStream.Position = 0;
        }

        // نسخ الـ stream لأن الأصلي قد يكون غير قابل للـ seek
        var memoryStream = new MemoryStream();
        imageStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        return Task.FromResult(new CompressedImageResult
        {
            Success = true,
            Stream = memoryStream,
            OriginalSize = size,
            CompressedSize = memoryStream.Length,
            OriginalWidth = 0,
            OriginalHeight = 0,
            NewWidth = 0,
            NewHeight = 0
        });
    }

    public async Task<CompressedImageResult> CompressFromFileAsync(
        string filePath,
        int maxWidth = 1920,
        int maxHeight = 1920,
        int quality = 80)
    {
        try
        {
            using var fileStream = File.OpenRead(filePath);
            return await CompressAsync(fileStream, maxWidth, maxHeight, quality);
        }
        catch (Exception ex)
        {
            return new CompressedImageResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }
}

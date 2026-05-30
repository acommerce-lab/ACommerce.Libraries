using ACommerce.Templates.Customer.Services;

namespace Nadena.Web.Services;

/// <summary>
/// Web implementation of IImageCompressionService.
/// WASM has no native image codec, so this is a pass-through (no resize/recompress):
/// it returns the original bytes so the upload flow keeps working in the browser.
/// </summary>
public class WebImageCompressionService : IImageCompressionService
{
    public async Task<CompressedImageResult> CompressAsync(
        Stream imageStream,
        int maxWidth = 1920,
        int maxHeight = 1920,
        int quality = 80)
    {
        try
        {
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            return new CompressedImageResult
            {
                Stream = new MemoryStream(bytes),
                OriginalSize = bytes.Length,
                CompressedSize = bytes.Length,
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new CompressedImageResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public Task<CompressedImageResult> CompressFromFileAsync(
        string filePath,
        int maxWidth = 1920,
        int maxHeight = 1920,
        int quality = 80)
        => Task.FromResult(new CompressedImageResult
        {
            Success = false,
            ErrorMessage = "CompressFromFile is not supported on web (no local file paths)."
        });
}

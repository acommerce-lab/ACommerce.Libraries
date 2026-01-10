using ACommerce.Templates.Customer.Services;
using SkiaSharp;

namespace Ashare.App.Services;

/// <summary>
/// خدمة ضغط الصور باستخدام SkiaSharp
/// تدعم Android و iOS و Windows و MacOS
/// </summary>
public class SkiaImageCompressionService : IImageCompressionService
{
    public async Task<CompressedImageResult> CompressAsync(
        Stream imageStream,
        int maxWidth = 1920,
        int maxHeight = 1920,
        int quality = 80)
    {
        var result = new CompressedImageResult();

        try
        {
            // قراءة الحجم الأصلي
            if (imageStream.CanSeek)
            {
                result.OriginalSize = imageStream.Length;
                imageStream.Position = 0;
            }

            // تحميل الصورة
            using var originalBitmap = SKBitmap.Decode(imageStream);
            if (originalBitmap == null)
            {
                result.Success = false;
                result.ErrorMessage = "فشل في قراءة الصورة";
                Console.WriteLine("[ImageCompression] Failed to decode image");
                return result;
            }

            result.OriginalWidth = originalBitmap.Width;
            result.OriginalHeight = originalBitmap.Height;

            Console.WriteLine($"[ImageCompression] Original: {originalBitmap.Width}x{originalBitmap.Height}, Size: {result.OriginalSize / 1024.0:F2} KB");

            // حساب الأبعاد الجديدة مع الحفاظ على النسبة
            var (newWidth, newHeight) = CalculateNewDimensions(
                originalBitmap.Width,
                originalBitmap.Height,
                maxWidth,
                maxHeight);

            result.NewWidth = newWidth;
            result.NewHeight = newHeight;

            SKBitmap resizedBitmap;

            // تغيير الحجم إذا لزم الأمر
            if (newWidth != originalBitmap.Width || newHeight != originalBitmap.Height)
            {
                resizedBitmap = originalBitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
                Console.WriteLine($"[ImageCompression] Resized to: {newWidth}x{newHeight}");
            }
            else
            {
                resizedBitmap = originalBitmap;
            }

            // ضغط كـ JPEG
            using var image = SKImage.FromBitmap(resizedBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);

            // إنشاء MemoryStream جديد
            var outputStream = new MemoryStream();
            data.SaveTo(outputStream);
            outputStream.Position = 0;

            result.Stream = outputStream;
            result.CompressedSize = outputStream.Length;
            result.Success = true;

            Console.WriteLine($"[ImageCompression] Compressed: {result.CompressedSize / 1024.0:F2} KB (saved {result.CompressionRatio:F1}%)");

            // تنظيف الذاكرة إذا تم إنشاء bitmap جديد
            if (resizedBitmap != originalBitmap)
            {
                resizedBitmap.Dispose();
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ImageCompression] Error: {ex.Message}");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task<CompressedImageResult> CompressFromFileAsync(
        string filePath,
        int maxWidth = 1920,
        int maxHeight = 1920,
        int quality = 80)
    {
        try
        {
            Console.WriteLine($"[ImageCompression] Compressing file: {filePath}");
            using var fileStream = File.OpenRead(filePath);
            return await CompressAsync(fileStream, maxWidth, maxHeight, quality);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ImageCompression] Error reading file: {ex.Message}");
            return new CompressedImageResult
            {
                Success = false,
                ErrorMessage = $"فشل في قراءة الملف: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// حساب الأبعاد الجديدة مع الحفاظ على نسبة العرض للارتفاع
    /// </summary>
    private static (int width, int height) CalculateNewDimensions(
        int originalWidth,
        int originalHeight,
        int maxWidth,
        int maxHeight)
    {
        // إذا كانت الصورة أصغر من الحد الأقصى، لا نغير شيئاً
        if (originalWidth <= maxWidth && originalHeight <= maxHeight)
        {
            return (originalWidth, originalHeight);
        }

        // حساب نسبة التصغير
        var widthRatio = (double)maxWidth / originalWidth;
        var heightRatio = (double)maxHeight / originalHeight;

        // نستخدم أصغر نسبة للحفاظ على الأبعاد ضمن الحدود
        var ratio = Math.Min(widthRatio, heightRatio);

        var newWidth = (int)(originalWidth * ratio);
        var newHeight = (int)(originalHeight * ratio);

        return (newWidth, newHeight);
    }
}

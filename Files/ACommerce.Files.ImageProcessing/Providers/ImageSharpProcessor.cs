// Providers/ImageSharpProcessor.cs
using ACommerce.Files.Abstractions.Enums;
using ACommerce.Files.Abstractions.Exceptions;
using ACommerce.Files.Abstractions.Models;
using ACommerce.Files.Abstractions.Providers;
using ACommerce.Files.ImageProcessing.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using ImageInfo = ACommerce.Files.Abstractions.Providers.ImageInfo;
using ImageProcessingException = ACommerce.Files.Abstractions.Exceptions.ImageProcessingException;

namespace ACommerce.Files.ImageProcessing.Providers;

public class ImageSharpProcessor : IImageProcessor
{
	private readonly ImageProcessorSettings _settings;
	private readonly ILogger<ImageSharpProcessor> _logger;

	public string ProviderName => "ImageSharp";

	public ImageSharpProcessor(
		IOptions<ImageProcessorSettings> settings,
		ILogger<ImageSharpProcessor> logger)
	{
		_settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async Task<Stream> ResizeAsync(
		Stream inputStream,
		int width,
		int height,
		bool maintainAspectRatio = true,
		CancellationToken cancellationToken = default)
	{
		try
		{
			using var image = await Image.LoadAsync(inputStream, cancellationToken);

			var resizeOptions = new ResizeOptions
			{
				Size = new Size(width, height),
				Mode = maintainAspectRatio ? ResizeMode.Max : ResizeMode.Stretch
			};

			image.Mutate(x => x.Resize(resizeOptions));

			var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder
			{
				Quality = _settings.DefaultQuality
			}, cancellationToken);

			outputStream.Position = 0;
			return outputStream;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to resize image");
			throw new ImageProcessingException("RESIZE_FAILED", "Failed to resize image", ex);
		}
	}

	public async Task<Stream> CreateThumbnailAsync(
		Stream inputStream,
		int size = 150,
		CancellationToken cancellationToken = default)
	{
		try
		{
			using var image = await Image.LoadAsync(inputStream, cancellationToken);

			image.Mutate(x => x.Resize(new ResizeOptions
			{
				Size = new Size(size, size),
				Mode = ResizeMode.Crop
			}));

			var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder
			{
				Quality = _settings.ThumbnailQuality
			}, cancellationToken);

			outputStream.Position = 0;
			return outputStream;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to create thumbnail");
			throw new ImageProcessingException("THUMBNAIL_FAILED", "Failed to create thumbnail", ex);
		}
	}

	public async Task<Stream> CropAsync(
		Stream inputStream,
		int x,
		int y,
		int width,
		int height,
		CancellationToken cancellationToken = default)
	{
		try
		{
			using var image = await Image.LoadAsync(inputStream, cancellationToken);

			var cropRectangle = new Rectangle(x, y, width, height);
			image.Mutate(i => i.Crop(cropRectangle));

			var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder
			{
				Quality = _settings.DefaultQuality
			}, cancellationToken);

			outputStream.Position = 0;
			return outputStream;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to crop image");
			throw new ImageProcessingException("CROP_FAILED", "Failed to crop image", ex);
		}
	}

	public async Task<Stream> AddWatermarkAsync(
		Stream inputStream,
		string watermarkText,
		CancellationToken cancellationToken = default)
	{
		try
		{
			using var image = await Image.LoadAsync(inputStream, cancellationToken);

			var fontFamily = SystemFonts.Families.FirstOrDefault();
			if (fontFamily.Equals(default(FontFamily)))
			{
				throw new Exception("No system fonts available");
			}
			var font = fontFamily.CreateFont(_settings.WatermarkFontSize, FontStyle.Bold);

			var textOptions = new RichTextOptions(font)
			{
				Origin = CalculateWatermarkPosition(image.Width, image.Height, watermarkText, font),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};

			var color = Color.White.WithAlpha(_settings.WatermarkOpacity);
			image.Mutate(x => x.DrawText(textOptions, watermarkText, color));

			var outputStream = new MemoryStream();
			await image.SaveAsJpegAsync(outputStream, new JpegEncoder
			{
				Quality = _settings.DefaultQuality
			}, cancellationToken);

			outputStream.Position = 0;
			return outputStream;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to add watermark");
			throw new ImageProcessingException("WATERMARK_FAILED", "Failed to add watermark", ex);
		}
	}

	public async Task<Stream> ConvertFormatAsync(
		Stream inputStream,
		ImageFormat format,
		int quality = 85,
		CancellationToken cancellationToken = default)
	{
		try
		{
			using var image = await Image.LoadAsync(inputStream, cancellationToken);

			var outputStream = new MemoryStream();

			IImageEncoder encoder = format switch
			{
				ImageFormat.Jpeg => new JpegEncoder { Quality = quality },
				ImageFormat.Png => new PngEncoder(),
				ImageFormat.WebP => new WebpEncoder { Quality = quality },
				_ => new JpegEncoder { Quality = quality }
			};

			await image.SaveAsync(outputStream, encoder, cancellationToken);

			outputStream.Position = 0;
			return outputStream;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to convert image format");
			throw new ImageProcessingException("CONVERT_FAILED", "Failed to convert image format", ex);
		}
	}

	public async Task<Stream> ProcessAsync(
		Stream inputStream,
		ImageProcessingOptions options, // ? ?? Abstractions.Models
		CancellationToken cancellationToken = default)
	{
		try
		{
			using var image = await Image.LoadAsync(inputStream, cancellationToken);

			// Resize
			if (options.Width.HasValue || options.Height.HasValue)
			{
				var width = options.Width ?? image.Width;
				var height = options.Height ?? image.Height;

				var resizeOptions = new ResizeOptions
				{
					Size = new Size(width, height),
					Mode = options.MaintainAspectRatio ? ResizeMode.Max : ResizeMode.Stretch
				};

				image.Mutate(x => x.Resize(resizeOptions));
			}

			// Watermark
			if (!string.IsNullOrWhiteSpace(options.WatermarkText))
			{
				var fontFamily = SystemFonts.Families.FirstOrDefault();
				if (fontFamily.Equals(default))
				{
					throw new Exception("No system fonts available");
				}
				var font = fontFamily.CreateFont(_settings.WatermarkFontSize, FontStyle.Bold);

				var textOptions = new RichTextOptions(font)
				{
					Origin = CalculateWatermarkPosition(image.Width, image.Height, options.WatermarkText, font),
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center
				};

				var color = Color.White.WithAlpha(_settings.WatermarkOpacity);
				image.Mutate(x => x.DrawText(textOptions, options.WatermarkText, color));
			}

			// Save with format
			var outputStream = new MemoryStream();
			var format = options.Format ?? ImageFormat.Jpeg;

			IImageEncoder encoder = format switch
			{
				ImageFormat.Jpeg => new JpegEncoder { Quality = options.Quality },
				ImageFormat.Png => new PngEncoder(),
				ImageFormat.WebP => new WebpEncoder { Quality = options.Quality },
				_ => new JpegEncoder { Quality = options.Quality }
			};

			await image.SaveAsync(outputStream, encoder, cancellationToken);

			outputStream.Position = 0;
			return outputStream;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to process image");
			throw new ImageProcessingException("PROCESS_FAILED", "Failed to process image", ex);
		}
	}

	public async Task<ImageInfo> GetImageInfoAsync(
		Stream inputStream,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var imageInfo = await Image.IdentifyAsync(inputStream, cancellationToken);

			if (imageInfo == null)
			{
				throw new ImageProcessingException("IDENTIFY_FAILED", "Failed to identify image");
			}

			return new ImageInfo
			{
				Width = imageInfo.Width,
				Height = imageInfo.Height,
				Format = imageInfo.Metadata.DecodedImageFormat?.Name ?? "Unknown",
				SizeInBytes = inputStream.Length
			};
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get image info");
			throw new ImageProcessingException("INFO_FAILED", "Failed to get image info", ex);
		}
	}

	private PointF CalculateWatermarkPosition(int imageWidth, int imageHeight, string text, Font font)
	{
		var textSize = TextMeasurer.MeasureSize(text, new RichTextOptions(font));
		var padding = 20;

		return _settings.WatermarkPosition switch
		{
			WatermarkPosition.TopLeft => new PointF(padding, padding),
			WatermarkPosition.TopCenter => new PointF(imageWidth / 2, padding),
			WatermarkPosition.TopRight => new PointF(imageWidth - textSize.Width - padding, padding),
			WatermarkPosition.MiddleLeft => new PointF(padding, imageHeight / 2),
			WatermarkPosition.MiddleCenter => new PointF(imageWidth / 2, imageHeight / 2),
			WatermarkPosition.MiddleRight => new PointF(imageWidth - textSize.Width - padding, imageHeight / 2),
			WatermarkPosition.BottomLeft => new PointF(padding, imageHeight - textSize.Height - padding),
			WatermarkPosition.BottomCenter => new PointF(imageWidth / 2, imageHeight - textSize.Height - padding),
			WatermarkPosition.BottomRight => new PointF(imageWidth - textSize.Width - padding, imageHeight - textSize.Height - padding),
			_ => new PointF(imageWidth - textSize.Width - padding, imageHeight - textSize.Height - padding)
		};
	}

	Task<ImageInfo> IImageProcessor.GetImageInfoAsync(Stream inputStream, CancellationToken cancellationToken) => throw new NotImplementedException();
}


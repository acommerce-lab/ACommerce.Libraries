// DTOs/Processing/ProcessImageDto.cs
using ACommerce.Files.Abstractions.Enums;
using System.ComponentModel.DataAnnotations;

namespace ACommerce.Files.AspNetCore.DTOs.Processing;

public class ProcessImageDto
{
	[Required(ErrorMessage = "File ID is required")]
	public string FileId { get; set; } = default!;

	[Range(50, 10000, ErrorMessage = "Width must be between 50 and 10000")]
	public int? Width { get; set; }

	[Range(50, 10000, ErrorMessage = "Height must be between 50 and 10000")]
	public int? Height { get; set; }

	public ImageFormat? Format { get; set; }

	[Range(1, 100, ErrorMessage = "Quality must be between 1 and 100")]
	public int Quality { get; set; } = 85;

	public bool MaintainAspectRatio { get; set; } = true;

	public string? WatermarkText { get; set; }
}

// DTOs/Processing/CropImageDto.cs
public class CropImageDto
{
	[Required(ErrorMessage = "File ID is required")]
	public string FileId { get; set; } = default!;

	[Required(ErrorMessage = "X coordinate is required")]
	[Range(0, int.MaxValue)]
	public int X { get; set; }

	[Required(ErrorMessage = "Y coordinate is required")]
	[Range(0, int.MaxValue)]
	public int Y { get; set; }

	[Required(ErrorMessage = "Width is required")]
	[Range(1, int.MaxValue)]
	public int Width { get; set; }

	[Required(ErrorMessage = "Height is required")]
	[Range(1, int.MaxValue)]
	public int Height { get; set; }
}


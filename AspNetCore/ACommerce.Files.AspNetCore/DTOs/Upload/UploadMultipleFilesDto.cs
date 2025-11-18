// DTOs/Upload/UploadMultipleFilesDto.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ACommerce.Files.AspNetCore.DTOs.Upload;

// DTOs/Upload/UploadMultipleFilesDto.cs
public class UploadMultipleFilesDto
{
	[Required(ErrorMessage = "At least one file is required")]
	[MinLength(1, ErrorMessage = "At least one file is required")]
	public List<IFormFile> Files { get; set; } = new();

	public string? Directory { get; set; }

	public bool GenerateThumbnail { get; set; } = true;

	public Dictionary<string, string>? Metadata { get; set; }
}


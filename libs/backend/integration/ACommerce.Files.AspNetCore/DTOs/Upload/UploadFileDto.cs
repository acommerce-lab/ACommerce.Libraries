// DTOs/Upload/UploadFileDto.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ACommerce.Files.AspNetCore.DTOs.Upload;

public class UploadFileDto
{
	[Required(ErrorMessage = "File is required")]
	public IFormFile File { get; set; } = default!;

	public string? Directory { get; set; }

	public bool GenerateThumbnail { get; set; } = true;

	public Dictionary<string, string>? Metadata { get; set; }
}


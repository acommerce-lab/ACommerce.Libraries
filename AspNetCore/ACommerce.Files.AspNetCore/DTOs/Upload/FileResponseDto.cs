// DTOs/Upload/FileResponseDto.cs
using ACommerce.Files.Abstractions.Enums;

namespace ACommerce.Files.AspNetCore.DTOs.Upload;

// DTOs/Upload/FileResponseDto.cs
public class FileResponseDto
{
	public string FileId { get; set; } = default!;
	public string FileName { get; set; } = default!;
	public string ContentType { get; set; } = default!;
	public long SizeInBytes { get; set; }
	public FileType FileType { get; set; }
	public string PublicUrl { get; set; } = default!;
	public string? ThumbnailUrl { get; set; }
	public int? Width { get; set; }
	public int? Height { get; set; }
	public Dictionary<string, string> Metadata { get; set; } = new();
	public DateTimeOffset UploadedAt { get; set; }
}


// Controllers/FilesController.cs
using ACommerce.Files.Abstractions.Helpers;
using ACommerce.Files.Abstractions.Models;
using ACommerce.Files.Abstractions.Providers;
using ACommerce.Files.AspNetCore.DTOs.Processing;
using ACommerce.Files.AspNetCore.DTOs.Upload;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ACommerce.Files.AspNetCore.Controllers;

/// <summary>
/// Controller ?????? ???????
/// </summary>
[ApiController]
[Route("api/files")]
[Authorize]
public class FilesController : ControllerBase
{
	private readonly IFileProvider _fileProvider;
	private readonly IImageProcessor? _imageProcessor;
	private readonly ILogger<FilesController> _logger;

	public FilesController(
		IFileProvider fileProvider,
		ILogger<FilesController> logger,
		IImageProcessor? imageProcessor = null)
	{
		_fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_imageProcessor = imageProcessor;
	}

	/// <summary>
	/// ??? ??? ????
	/// </summary>
	[HttpPost("upload")]
	public async Task<IActionResult> UploadFile(
		[FromForm] UploadFileDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var userId = User.FindFirst("sub")?.Value
				?? User.FindFirst("userId")?.Value
				?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			_logger.LogInformation("Uploading file: {FileName} by user: {UserId}",
				dto.File.FileName, userId);

			var stream = dto.File.OpenReadStream();

			var request = new UploadRequest
			{
				FileStream = stream,
				FileName = dto.File.FileName,
				ContentType = dto.File.ContentType,
				OwnerId = userId,
				Directory = dto.Directory,
				GenerateThumbnail = dto.GenerateThumbnail && FileTypeHelper.IsImage(dto.File.ContentType),
				Metadata = dto.Metadata
			};

			var result = await _fileProvider.UploadAsync(request, cancellationToken);

			if (!result.Success)
			{
				return BadRequest(new
				{
					error = result.Error?.Code,
					message = result.Error?.Message,
					details = result.Error?.Details
				});
			}

			return CreatedAtAction(
				nameof(GetFile),
				new { fileId = result.File!.FileId },
				MapToFileResponseDto(result.File));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error uploading file");
			return StatusCode(500, new
			{
				error = "UPLOAD_FAILED",
				message = "Failed to upload file"
			});
		}
	}

	/// <summary>
	/// ??? ??? ?????
	/// </summary>
	[HttpPost("upload-multiple")]
	public async Task<IActionResult> UploadMultipleFiles(
		[FromForm] UploadMultipleFilesDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var userId = User.FindFirst("sub")?.Value
				?? User.FindFirst("userId")?.Value
				?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			_logger.LogInformation("Uploading {Count} files by user: {UserId}",
				dto.Files.Count, userId);

			var results = new List<FileResponseDto>();

			foreach (var file in dto.Files)
			{
				var stream = file.OpenReadStream();

				var request = new UploadRequest
				{
					FileStream = stream,
					FileName = file.FileName,
					ContentType = file.ContentType,
					OwnerId = userId,
					Directory = dto.Directory,
					GenerateThumbnail = dto.GenerateThumbnail && FileTypeHelper.IsImage(file.ContentType),
					Metadata = dto.Metadata
				};

				var result = await _fileProvider.UploadAsync(request, cancellationToken);

				if (result.Success && result.File != null)
				{
					results.Add(MapToFileResponseDto(result.File));
				}
			}

			return Ok(new
			{
				uploaded = results.Count,
				total = dto.Files.Count,
				files = results
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error uploading multiple files");
			return StatusCode(500, new
			{
				error = "UPLOAD_FAILED",
				message = "Failed to upload files"
			});
		}
	}

	/// <summary>
	/// ??? ??????? ???
	/// </summary>
	[HttpGet("{fileId}")]
	public async Task<IActionResult> GetFile(
		string fileId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var file = await _fileProvider.GetFileAsync(fileId, cancellationToken);

			if (file == null)
			{
				return NotFound(new
				{
					error = "FILE_NOT_FOUND",
					message = "File not found"
				});
			}

			return Ok(MapToFileResponseDto(file));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting file: {FileId}", fileId);
			return StatusCode(500, new
			{
				error = "GET_FILE_FAILED",
				message = "Failed to get file"
			});
		}
	}

	/// <summary>
	/// ????? ???
	/// </summary>
	[HttpGet("{fileId}/download")]
	[AllowAnonymous]
	public async Task<IActionResult> DownloadFile(
		string fileId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var file = await _fileProvider.GetFileAsync(fileId, cancellationToken);

			if (file == null)
			{
				return NotFound(new
				{
					error = "FILE_NOT_FOUND",
					message = "File not found"
				});
			}

			var stream = await _fileProvider.DownloadAsync(fileId, cancellationToken);

			if (stream == null)
			{
				return NotFound(new
				{
					error = "FILE_NOT_FOUND",
					message = "File content not found"
				});
			}

			return File(stream, file.ContentType, file.FileName);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error downloading file: {FileId}", fileId);
			return StatusCode(500, new
			{
				error = "DOWNLOAD_FAILED",
				message = "Failed to download file"
			});
		}
	}

	/// <summary>
	/// ??? ???
	/// </summary>
	[HttpDelete("{fileId}")]
	public async Task<IActionResult> DeleteFile(
		string fileId,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var userId = User.FindFirst("sub")?.Value
				?? User.FindFirst("userId")?.Value
				?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			// ?????? ?? ???????
			var file = await _fileProvider.GetFileAsync(fileId, cancellationToken);

			if (file == null)
			{
				return NotFound(new
				{
					error = "FILE_NOT_FOUND",
					message = "File not found"
				});
			}

			if (file.OwnerId != userId && !User.IsInRole("Admin"))
			{
				return Forbid();
			}

			_logger.LogInformation("Deleting file: {FileId} by user: {UserId}", fileId, userId);

			var success = await _fileProvider.DeleteAsync(fileId, cancellationToken);

			if (!success)
			{
				return BadRequest(new
				{
					error = "DELETE_FAILED",
					message = "Failed to delete file"
				});
			}

			return NoContent();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting file: {FileId}", fileId);
			return StatusCode(500, new
			{
				error = "DELETE_FAILED",
				message = "Failed to delete file"
			});
		}
	}

	/// <summary>
	/// ??? ????? ???????? ??????
	/// </summary>
	[HttpGet("my-files")]
	public async Task<IActionResult> GetMyFiles(CancellationToken cancellationToken = default)
	{
		try
		{
			var userId = User.FindFirst("sub")?.Value
				?? User.FindFirst("userId")?.Value
				?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized();
			}

			var files = await _fileProvider.GetUserFilesAsync(userId, cancellationToken);

			return Ok(new
			{
				count = files.Count,
				files = files.Select(MapToFileResponseDto).ToList()
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting user files");
			return StatusCode(500, new
			{
				error = "GET_FILES_FAILED",
				message = "Failed to get files"
			});
		}
	}

	/// <summary>
	/// ????? ?? ???????
	/// </summary>
	[HttpGet("search")]
	public async Task<IActionResult> SearchFiles(
		[FromQuery] string query,
		[FromQuery] Abstractions.Enums.FileType? fileType = null,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var files = await _fileProvider.SearchAsync(query, fileType, cancellationToken);

			return Ok(new
			{
				count = files.Count,
				query,
				fileType,
				files = files.Select(MapToFileResponseDto).ToList()
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error searching files");
			return StatusCode(500, new
			{
				error = "SEARCH_FAILED",
				message = "Failed to search files"
			});
		}
	}

	/// <summary>
	/// ?????? ????
	/// </summary>
	[HttpPost("process-image")]
	public async Task<IActionResult> ProcessImage(
		[FromBody] ProcessImageDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			if (_imageProcessor == null)
			{
				return BadRequest(new
				{
					error = "IMAGE_PROCESSOR_NOT_AVAILABLE",
					message = "Image processing is not available"
				});
			}

			var file = await _fileProvider.GetFileAsync(dto.FileId, cancellationToken);

			if (file == null)
			{
				return NotFound(new
				{
					error = "FILE_NOT_FOUND",
					message = "File not found"
				});
			}

			if (!FileTypeHelper.IsImage(file.ContentType))
			{
				return BadRequest(new
				{
					error = "NOT_AN_IMAGE",
					message = "File is not an image"
				});
			}

			var stream = await _fileProvider.DownloadAsync(dto.FileId, cancellationToken);

			if (stream == null)
			{
				return NotFound(new
				{
					error = "FILE_CONTENT_NOT_FOUND",
					message = "File content not found"
				});
			}

			var options = new ImageProcessingOptions
			{
				Width = dto.Width,
				Height = dto.Height,
				Format = dto.Format,
				Quality = dto.Quality,
				MaintainAspectRatio = dto.MaintainAspectRatio,
				WatermarkText = dto.WatermarkText
			};

			var processedStream = await _imageProcessor.ProcessAsync(stream, options, cancellationToken);

			return File(processedStream, file.ContentType, $"processed_{file.FileName}");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error processing image");
			return StatusCode(500, new
			{
				error = "PROCESS_FAILED",
				message = "Failed to process image"
			});
		}
	}

	/// <summary>
	/// ?? ????
	/// </summary>
	[HttpPost("crop-image")]
	public async Task<IActionResult> CropImage(
		[FromBody] CropImageDto dto,
		CancellationToken cancellationToken = default)
	{
		try
		{
			if (_imageProcessor == null)
			{
				return BadRequest(new
				{
					error = "IMAGE_PROCESSOR_NOT_AVAILABLE",
					message = "Image processing is not available"
				});
			}

			var file = await _fileProvider.GetFileAsync(dto.FileId, cancellationToken);

			if (file == null)
			{
				return NotFound(new
				{
					error = "FILE_NOT_FOUND",
					message = "File not found"
				});
			}

			if (!FileTypeHelper.IsImage(file.ContentType))
			{
				return BadRequest(new
				{
					error = "NOT_AN_IMAGE",
					message = "File is not an image"
				});
			}

			var stream = await _fileProvider.DownloadAsync(dto.FileId, cancellationToken);

			if (stream == null)
			{
				return NotFound(new
				{
					error = "FILE_CONTENT_NOT_FOUND",
					message = "File content not found"
				});
			}

			var croppedStream = await _imageProcessor.CropAsync(
				stream,
				dto.X,
				dto.Y,
				dto.Width,
				dto.Height,
				cancellationToken);

			return File(croppedStream, file.ContentType, $"cropped_{file.FileName}");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error cropping image");
			return StatusCode(500, new
			{
				error = "CROP_FAILED",
				message = "Failed to crop image"
			});
		}
	}

	private FileResponseDto MapToFileResponseDto(Abstractions.Models.FileInfo file)
	{
		return new FileResponseDto
		{
			FileId = file.FileId,
			FileName = file.FileName,
			ContentType = file.ContentType,
			SizeInBytes = file.SizeInBytes,
			FileType = file.FileType,
			PublicUrl = file.PublicUrl,
			ThumbnailUrl = file.ThumbnailUrl,
			Width = file.Width,
			Height = file.Height,
			Metadata = file.Metadata,
			UploadedAt = file.UploadedAt
		};
	}
}


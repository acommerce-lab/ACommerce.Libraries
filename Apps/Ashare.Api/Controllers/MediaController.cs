using ACommerce.Files.Abstractions.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ashare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IStorageProvider _storageProvider;
    private readonly ILogger<MediaController> _logger;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp"
    };

    private static readonly HashSet<string> AllowedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        "listings",
        "profiles",
        "vendors"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public MediaController(
        IStorageProvider storageProvider,
        ILogger<MediaController> logger)
    {
        _storageProvider = storageProvider;
        _logger = logger;
    }

    [HttpPost("upload")]
    [Authorize]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string? directory = "listings", CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No file provided" });
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { error = $"File size exceeds maximum allowed ({MaxFileSizeBytes / 1024 / 1024}MB)" });
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return BadRequest(new { error = $"Content type '{file.ContentType}' is not allowed. Allowed types: {string.Join(", ", AllowedContentTypes)}" });
        }

        var safeDirectory = string.IsNullOrWhiteSpace(directory) ? "listings" : directory;
        if (!AllowedDirectories.Contains(safeDirectory))
        {
            return BadRequest(new { error = $"Directory '{safeDirectory}' is not allowed. Allowed: {string.Join(", ", AllowedDirectories)}" });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var objectName = await _storageProvider.SaveAsync(stream, file.FileName, safeDirectory, cancellationToken);
            var publicUrl = await _storageProvider.GetPublicUrlAsync(objectName, cancellationToken);

            _logger.LogInformation("File uploaded successfully: {ObjectName}", objectName);

            return Ok(new UploadResponse
            {
                ObjectName = objectName,
                Url = publicUrl,
                ContentType = file.ContentType,
                Size = file.Length
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file: {FileName}", file.FileName);
            return StatusCode(500, new { error = "Failed to upload file" });
        }
    }

    [HttpPost("upload/multiple")]
    [Authorize]
    [RequestSizeLimit(MaxFileSizeBytes * 5)]
    public async Task<IActionResult> UploadMultiple(List<IFormFile> files, [FromQuery] string? directory = "listings", CancellationToken cancellationToken = default)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(new { error = "No files provided" });
        }

        if (files.Count > 5)
        {
            return BadRequest(new { error = "Maximum 5 files allowed per request" });
        }

        var safeDirectory = string.IsNullOrWhiteSpace(directory) ? "listings" : directory;
        if (!AllowedDirectories.Contains(safeDirectory))
        {
            return BadRequest(new { error = $"Directory '{safeDirectory}' is not allowed. Allowed: {string.Join(", ", AllowedDirectories)}" });
        }

        var results = new List<UploadResponse>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            if (file.Length == 0)
            {
                errors.Add($"{file.FileName}: Empty file");
                continue;
            }

            if (file.Length > MaxFileSizeBytes)
            {
                errors.Add($"{file.FileName}: File too large");
                continue;
            }

            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                errors.Add($"{file.FileName}: Invalid content type");
                continue;
            }

            try
            {
                await using var stream = file.OpenReadStream();
                var objectName = await _storageProvider.SaveAsync(stream, file.FileName, safeDirectory, cancellationToken);
                var publicUrl = await _storageProvider.GetPublicUrlAsync(objectName, cancellationToken);

                results.Add(new UploadResponse
                {
                    ObjectName = objectName,
                    Url = publicUrl,
                    ContentType = file.ContentType,
                    Size = file.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload file: {FileName}", file.FileName);
                errors.Add($"{file.FileName}: Upload failed");
            }
        }

        return Ok(new
        {
            uploaded = results,
            errors = errors
        });
    }


    public class UploadResponse
    {
        public string ObjectName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}

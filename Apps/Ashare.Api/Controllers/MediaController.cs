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
        _logger.LogInformation("=== [MediaController] Upload Request ===");
        _logger.LogInformation("[MediaController] Authorization Header: {AuthHeader}", 
            Request.Headers.Authorization.FirstOrDefault() ?? "NOT PRESENT");
        _logger.LogInformation("[MediaController] User.Identity.IsAuthenticated: {IsAuth}", 
            User?.Identity?.IsAuthenticated ?? false);
        _logger.LogInformation("[MediaController] User.Identity.Name: {Name}", 
            User?.Identity?.Name ?? "NULL");
        _logger.LogInformation("[MediaController] Directory: {Directory}", directory);
        _logger.LogInformation("[MediaController] File: {FileName}, Size: {Size}, ContentType: {ContentType}", 
            file?.FileName ?? "NULL", file?.Length ?? 0, file?.ContentType ?? "NULL");
        
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("[MediaController] No file provided");
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

            // Use X-Forwarded-Proto header if available (for load balancers/reverse proxies like Cloud Run)
            // Otherwise fall back to Request.Scheme, but prefer HTTPS in production
            var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? Request.Scheme;
            if (scheme == "http" && !Request.Host.Host.Contains("localhost") && !Request.Host.Host.Contains("127.0.0.1"))
            {
                scheme = "https"; // Force HTTPS for production environments
            }
            var baseUrl = $"{scheme}://{Request.Host}";
            var proxyUrl = $"{baseUrl}/api/media/{objectName}";

            _logger.LogInformation("File uploaded successfully: {ObjectName}, ProxyUrl: {ProxyUrl}", objectName, proxyUrl);

            return Ok(new UploadResponse
            {
                ObjectName = objectName,
                Url = proxyUrl,
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

                // Use X-Forwarded-Proto header if available (for load balancers/reverse proxies like Cloud Run)
                var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? Request.Scheme;
                if (scheme == "http" && !Request.Host.Host.Contains("localhost") && !Request.Host.Host.Contains("127.0.0.1"))
                {
                    scheme = "https";
                }
                var baseUrl = $"{scheme}://{Request.Host}";
                var proxyUrl = $"{baseUrl}/api/media/{objectName}";

                results.Add(new UploadResponse
                {
                    ObjectName = objectName,
                    Url = proxyUrl,
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


    [HttpGet("{directory}/{fileName}")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetImage(string directory, string fileName, CancellationToken cancellationToken = default)
    {
        if (!AllowedDirectories.Contains(directory))
        {
            return NotFound();
        }

        var objectName = $"{directory}/{fileName}";

        try
        {
            var stream = await _storageProvider.GetAsync(objectName, cancellationToken);
            if (stream == null)
            {
                return NotFound();
            }

            var contentType = GetContentType(fileName);
            return File(stream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file: {ObjectName}", objectName);
            return NotFound();
        }
    }

    /// <summary>
    /// جلب صورة من مسار متداخل (مثل listings/migrated/file.jpg)
    /// </summary>
    [HttpGet("{directory}/{subDirectory}/{fileName}")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetNestedImage(string directory, string subDirectory, string fileName, CancellationToken cancellationToken = default)
    {
        if (!AllowedDirectories.Contains(directory))
        {
            return NotFound();
        }

        var objectName = $"{directory}/{subDirectory}/{fileName}";

        try
        {
            _logger.LogDebug("Getting nested image: {ObjectName}", objectName);
            var stream = await _storageProvider.GetAsync(objectName, cancellationToken);
            if (stream == null)
            {
                _logger.LogWarning("Image not found in GCS: {ObjectName}", objectName);
                return NotFound();
            }

            var contentType = GetContentType(fileName);
            return File(stream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get nested file: {ObjectName}", objectName);
            return NotFound();
        }
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    public class UploadResponse
    {
        public string ObjectName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}

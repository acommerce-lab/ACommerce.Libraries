using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ACommerce.Client.Core.Http;
using ACommerce.Client.Core.Interceptors;
using ACommerce.ServiceRegistry.Client;

namespace ACommerce.Client.Files;

public sealed class FilesClient
{
        private readonly IApiClient _httpClient;
        private readonly HttpClient _rawHttpClient;
        private readonly ITokenProvider? _tokenProvider;
        private readonly ServiceRegistryClient _serviceRegistry;
        private readonly IDeviceInfoProvider _deviceInfo;
        private const string ServiceName = "Files";
        private static readonly TimeSpan UploadTimeout = TimeSpan.FromSeconds(180); // 3 minutes timeout

        public FilesClient(
                IApiClient httpClient,
                IHttpClientFactory httpClientFactory,
                ServiceRegistryClient serviceRegistry,
                ITokenProvider? tokenProvider = null,
                IDeviceInfoProvider? deviceInfoProvider = null)
        {
                _httpClient = httpClient;
                _rawHttpClient = httpClientFactory.CreateClient("DynamicHttpClient");
                // Ensure HttpClient timeout is set correctly (fixes 60-second default timeout issue)
                _rawHttpClient.Timeout = UploadTimeout;
                Console.WriteLine($"[FilesClient] HttpClient timeout set to {_rawHttpClient.Timeout.TotalSeconds}s");
                _serviceRegistry = serviceRegistry;
                _tokenProvider = tokenProvider;
                _deviceInfo = deviceInfoProvider ?? new DefaultDeviceInfoProvider();
        }
        
        private async Task<string> GetServiceUrlAsync(CancellationToken cancellationToken = default)
        {
                Console.WriteLine($"[FilesClient] GetServiceUrlAsync - Discovering service: {ServiceName}");
                var endpoint = await _serviceRegistry.DiscoverAsync(ServiceName, cancellationToken);
                if (endpoint == null)
                {
                        Console.WriteLine($"[FilesClient] ERROR: Service not found: {ServiceName}");
                        throw new InvalidOperationException($"Service not found: {ServiceName}");
                }
                var baseUrl = endpoint.BaseUrl.TrimEnd('/');
                Console.WriteLine($"[FilesClient] Service discovered: {ServiceName} -> {baseUrl}");
                return baseUrl;
        }

        private async Task AddAuthorizationAsync(HttpRequestMessage request)
        {
                Console.WriteLine($"[FilesClient] AddAuthorizationAsync - TokenProvider is {(_tokenProvider != null ? "available" : "NULL")}");
                if (_tokenProvider != null)
                {
                        var token = await _tokenProvider.GetTokenAsync();
                        Console.WriteLine($"[FilesClient] Token from provider: {(string.IsNullOrEmpty(token) ? "EMPTY/NULL" : $"Bearer {token.Substring(0, Math.Min(20, token.Length))}...")}");
                        if (!string.IsNullOrEmpty(token))
                        {
                                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                Console.WriteLine($"[FilesClient] Authorization header SET");
                        }
                        else
                        {
                                Console.WriteLine($"[FilesClient] WARNING: Token is empty, Authorization header NOT set");
                        }
                }
                else
                {
                        Console.WriteLine($"[FilesClient] WARNING: TokenProvider is null, Authorization header NOT set");
                }
        }

        /// <summary>
        /// رفع ملف
        /// </summary>
        public async Task<FileUploadResponse?> UploadFileAsync(
                Stream fileStream,
                string fileName,
                string? folder = null,
                CancellationToken cancellationToken = default)
        {
                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(fileStream);

                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                content.Add(fileContent, "file", fileName);

                if (!string.IsNullOrEmpty(folder))
                {
                        content.Add(new StringContent(folder), "folder");
                }

                // نحتاج URL الخدمة يدوياً هنا لأن DynamicHttpClient لا يدعم MultipartFormData
                var response = await _rawHttpClient.PostAsync($"/api/files/upload", content, cancellationToken);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<FileUploadResponse>(cancellationToken);
        }

        /// <summary>
        /// رفع صورة
        /// </summary>
        public async Task<FileUploadResponse?> UploadImageAsync(
                Stream imageStream,
                string fileName,
                ImageUploadOptions? options = null,
                CancellationToken cancellationToken = default)
        {
                options ??= new ImageUploadOptions();

                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(imageStream);

                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                content.Add(fileContent, "file", fileName);
                content.Add(new StringContent(options.Folder), "folder");
                content.Add(new StringContent(options.GenerateThumbnail.ToString()), "generateThumbnail");

                if (options.MaxWidth.HasValue)
                {
                        content.Add(new StringContent(options.MaxWidth.Value.ToString()), "maxWidth");
                }

                if (options.MaxHeight.HasValue)
                {
                        content.Add(new StringContent(options.MaxHeight.Value.ToString()), "maxHeight");
                }

                var response = await _rawHttpClient.PostAsync($"/api/files/upload-image", content, cancellationToken);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<FileUploadResponse>(cancellationToken);
        }

        /// <summary>
        /// رفع صورة للإعلانات عبر Media API (Google Cloud Storage)
        /// </summary>
        public async Task<MediaUploadResponse?> UploadMediaAsync(
                Stream imageStream,
                string fileName,
                string contentType,
                string directory = "listings",
                CancellationToken cancellationToken = default)
        {
                var startTime = DateTime.UtcNow;
                long streamLength = 0;
                string? baseUrl = null;

                try
                {
                        // Get stream length for logging
                        if (imageStream.CanSeek)
                        {
                                streamLength = imageStream.Length;
                                Console.WriteLine($"[FilesClient] UploadMediaAsync - Stream length: {streamLength / 1024.0:F2} KB");
                        }

                        using var content = new MultipartFormDataContent();
                        using var fileContent = new StreamContent(imageStream);

                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                        content.Add(fileContent, "file", fileName);

                        baseUrl = await GetServiceUrlAsync(cancellationToken);
                        var fullUrl = $"{baseUrl}/api/media/upload?directory={directory}";
                        Console.WriteLine($"[FilesClient] UploadMediaAsync - URL: {fullUrl}");
                        Console.WriteLine($"[FilesClient] UploadMediaAsync - FileName: {fileName}, ContentType: {contentType}");

                        var request = new HttpRequestMessage(HttpMethod.Post, fullUrl)
                        {
                                Content = content
                        };
                        await AddAuthorizationAsync(request);

                        Console.WriteLine($"[FilesClient] UploadMediaAsync - Sending request...");

                        // Use timeout for upload
                        using var timeoutCts = new CancellationTokenSource(UploadTimeout);
                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                        var response = await _rawHttpClient.SendAsync(request, linkedCts.Token);
                        var duration = DateTime.UtcNow - startTime;
                        Console.WriteLine($"[FilesClient] UploadMediaAsync - Response: {(int)response.StatusCode} {response.StatusCode} (took {duration.TotalSeconds:F2}s)");

                        if (!response.IsSuccessStatusCode)
                        {
                                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                                Console.WriteLine($"[FilesClient] UploadMediaAsync - ERROR Body: {errorBody}");

                                // Report error
                                await ReportErrorAsync("FilesClient", "UploadMediaAsync",
                                        $"HTTP {(int)response.StatusCode}: {errorBody}",
                                        new Dictionary<string, object>
                                        {
                                                ["fileName"] = fileName,
                                                ["contentType"] = contentType,
                                                ["directory"] = directory,
                                                ["streamLength"] = streamLength,
                                                ["duration"] = duration.TotalSeconds,
                                                ["statusCode"] = (int)response.StatusCode,
                                                ["url"] = fullUrl
                                        });

                                response.EnsureSuccessStatusCode();
                        }

                        return await response.Content.ReadFromJsonAsync<MediaUploadResponse>(cancellationToken);
                }
                catch (OperationCanceledException ex) when (ex.CancellationToken != cancellationToken)
                {
                        // Timeout occurred
                        var duration = DateTime.UtcNow - startTime;
                        Console.WriteLine($"[FilesClient] UploadMediaAsync - TIMEOUT after {duration.TotalSeconds:F2}s");

                        await ReportErrorAsync("FilesClient", "UploadMediaAsync",
                                $"Upload timeout after {duration.TotalSeconds:F2}s",
                                new Dictionary<string, object>
                                {
                                        ["fileName"] = fileName,
                                        ["contentType"] = contentType,
                                        ["directory"] = directory,
                                        ["streamLength"] = streamLength,
                                        ["duration"] = duration.TotalSeconds,
                                        ["timeoutSeconds"] = UploadTimeout.TotalSeconds
                                });

                        throw new TimeoutException($"Upload timed out after {UploadTimeout.TotalSeconds} seconds");
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                        var duration = DateTime.UtcNow - startTime;
                        Console.WriteLine($"[FilesClient] UploadMediaAsync - EXCEPTION: {ex.GetType().Name}: {ex.Message}");

                        await ReportErrorAsync("FilesClient", "UploadMediaAsync",
                                $"{ex.GetType().Name}: {ex.Message}",
                                new Dictionary<string, object>
                                {
                                        ["fileName"] = fileName,
                                        ["contentType"] = contentType,
                                        ["directory"] = directory,
                                        ["streamLength"] = streamLength,
                                        ["duration"] = duration.TotalSeconds,
                                        ["exceptionType"] = ex.GetType().FullName ?? ex.GetType().Name
                                },
                                ex.StackTrace);

                        throw;
                }
        }

        /// <summary>
        /// إرسال تقرير خطأ للخادم
        /// </summary>
        private async Task ReportErrorAsync(string source, string operation, string errorMessage,
                Dictionary<string, object>? additionalData = null, string? stackTrace = null)
        {
                try
                {
                        var baseUrl = await _serviceRegistry.DiscoverAsync(ServiceName, CancellationToken.None);
                        if (baseUrl == null) return;

                        // إضافة معلومات الشبكة للبيانات الإضافية
                        additionalData ??= new Dictionary<string, object>();
                        additionalData["networkType"] = _deviceInfo.NetworkType;
                        additionalData["manufacturer"] = _deviceInfo.Manufacturer;

                        var report = new
                        {
                                reportId = Guid.NewGuid().ToString(),
                                source,
                                operation,
                                errorMessage,
                                stackTrace,
                                platform = _deviceInfo.Platform,
                                appVersion = _deviceInfo.AppVersion,
                                osVersion = _deviceInfo.OsVersion,
                                deviceModel = _deviceInfo.DeviceModel,
                                timestamp = DateTime.UtcNow,
                                additionalData
                        };

                        var json = JsonSerializer.Serialize(report);
                        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                        // Fire and forget - don't wait for response
                        _ = _rawHttpClient.PostAsync($"{baseUrl.BaseUrl}/api/errorreporting/report", content);

                        Console.WriteLine($"[FilesClient] Error report sent for {operation}");
                }
                catch (Exception ex)
                {
                        Console.WriteLine($"[FilesClient] Failed to send error report: {ex.Message}");
                }
        }

        /// <summary>
        /// رفع صور متعددة عبر Media API (Google Cloud Storage)
        /// </summary>
        public async Task<MultipleMediaUploadResponse?> UploadMultipleMediaAsync(
                IEnumerable<(Stream Stream, string FileName, string ContentType)> files,
                string directory = "listings",
                CancellationToken cancellationToken = default)
        {
                using var content = new MultipartFormDataContent();
                var streamList = new List<StreamContent>();

                foreach (var (stream, fileName, contentType) in files)
                {
                        var fileContent = new StreamContent(stream);
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
                        content.Add(fileContent, "files", fileName);
                        streamList.Add(fileContent);
                }

                try
                {
                        var baseUrl = await GetServiceUrlAsync(cancellationToken);
                        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/media/upload/multiple?directory={directory}")
                        {
                                Content = content
                        };
                        await AddAuthorizationAsync(request);
                        
                        var response = await _rawHttpClient.SendAsync(request, cancellationToken);
                        response.EnsureSuccessStatusCode();

                        return await response.Content.ReadFromJsonAsync<MultipleMediaUploadResponse>(cancellationToken);
                }
                finally
                {
                        foreach (var sc in streamList)
                        {
                                sc.Dispose();
                        }
                }
        }

        /// <summary>
        /// حذف ملف
        /// </summary>
        public async Task DeleteFileAsync(
                string fileId,
                CancellationToken cancellationToken = default)
        {
                await _httpClient.DeleteAsync(
                        ServiceName,
                        $"/api/files/{fileId}",
                        cancellationToken);
        }

        /// <summary>
        /// الحصول على معلومات ملف
        /// </summary>
        public async Task<FileInfoResponse?> GetFileInfoAsync(
                string fileId,
                CancellationToken cancellationToken = default)
        {
                return await _httpClient.GetAsync<FileInfoResponse>(
                        ServiceName,
                        $"/api/files/{fileId}",
                        cancellationToken);
        }
}

public sealed class FileUploadResponse
{
        public string FileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; } = string.Empty;
}

public sealed class FileInfoResponse
{
        public string FileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public long Size { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
}

public sealed class ImageUploadOptions
{
        public string Folder { get; set; } = "images";
        public bool GenerateThumbnail { get; set; } = true;
        public int? MaxWidth { get; set; }
        public int? MaxHeight { get; set; }
}

public sealed class MediaUploadResponse
{
        public string ObjectName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
}

public sealed class MultipleMediaUploadResponse
{
        public List<MediaUploadResponse> Uploaded { get; set; } = new();
        public List<string> Errors { get; set; } = new();
}

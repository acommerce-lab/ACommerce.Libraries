using System.Net.Http.Headers;
using System.Net.Http.Json;
using ACommerce.Client.Core.Http;

namespace ACommerce.Client.Files;

public sealed class FilesClient
{
	private readonly DynamicHttpClient _httpClient;
	private readonly HttpClient _rawHttpClient;
	private const string ServiceName = "Files"; // أو "Marketplace"

	public FilesClient(DynamicHttpClient httpClient, IHttpClientFactory httpClientFactory)
	{
		_httpClient = httpClient;
		_rawHttpClient = httpClientFactory.CreateClient();
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

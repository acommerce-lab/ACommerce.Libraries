using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ACommerce.Client.Core.Http;

/// <summary>
/// HTTP Client مع Static Base URL
/// للتطبيقات المستقلة التي لا تستخدم Service Registry
/// </summary>
public sealed class StaticHttpClient : IApiClient
{
	private readonly HttpClient _httpClient;
	private readonly string _baseUrl;
	private readonly ILogger<StaticHttpClient>? _logger;
	private readonly JsonSerializerOptions _jsonOptions;

	public StaticHttpClient(
		HttpClient httpClient,
		string baseUrl,
		ILogger<StaticHttpClient>? logger = null)
	{
		_httpClient = httpClient;
		_baseUrl = baseUrl.TrimEnd('/');
		_logger = logger;

		_jsonOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
		};
	}

	/// <summary>
	/// GET Request
	/// </summary>
	public async Task<T?> GetAsync<T>(
		string serviceName,
		string path,
		CancellationToken cancellationToken = default)
	{
		var url = BuildUrl(path);
		_logger?.LogDebug("GET {Url}", url);

		try
		{
			var response = await _httpClient.GetAsync(url, cancellationToken);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "GET {Url} failed", url);
			throw;
		}
	}

	/// <summary>
	/// POST Request
	/// </summary>
	public async Task<TResponse?> PostAsync<TRequest, TResponse>(
		string serviceName,
		string path,
		TRequest data,
		CancellationToken cancellationToken = default)
	{
		var url = BuildUrl(path);
		_logger?.LogDebug("POST {Url}", url);

		try
		{
			var response = await _httpClient.PostAsJsonAsync(url, data, _jsonOptions, cancellationToken);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "POST {Url} failed", url);
			throw;
		}
	}

	/// <summary>
	/// POST Request بدون Response
	/// </summary>
	public async Task PostAsync<TRequest>(
		string serviceName,
		string path,
		TRequest data,
		CancellationToken cancellationToken = default)
	{
		var url = BuildUrl(path);
		_logger?.LogDebug("POST {Url}", url);

		try
		{
			var response = await _httpClient.PostAsJsonAsync(url, data, _jsonOptions, cancellationToken);
			response.EnsureSuccessStatusCode();
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "POST {Url} failed", url);
			throw;
		}
	}

	/// <summary>
	/// PUT Request
	/// </summary>
	public async Task<TResponse?> PutAsync<TRequest, TResponse>(
		string serviceName,
		string path,
		TRequest data,
		CancellationToken cancellationToken = default)
	{
		var url = BuildUrl(path);
		_logger?.LogDebug("PUT {Url}", url);

		try
		{
			var response = await _httpClient.PutAsJsonAsync(url, data, _jsonOptions, cancellationToken);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "PUT {Url} failed", url);
			throw;
		}
	}

	/// <summary>
	/// PUT Request بدون Response
	/// </summary>
	public async Task PutAsync<TRequest>(
		string serviceName,
		string path,
		TRequest data,
		CancellationToken cancellationToken = default)
	{
		var url = BuildUrl(path);
		_logger?.LogDebug("PUT {Url}", url);

		try
		{
			var response = await _httpClient.PutAsJsonAsync(url, data, _jsonOptions, cancellationToken);
			response.EnsureSuccessStatusCode();
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "PUT {Url} failed", url);
			throw;
		}
	}

	/// <summary>
	/// DELETE Request
	/// </summary>
	public async Task DeleteAsync(
		string serviceName,
		string path,
		CancellationToken cancellationToken = default)
	{
		var url = BuildUrl(path);
		_logger?.LogDebug("DELETE {Url}", url);

		try
		{
			var response = await _httpClient.DeleteAsync(url, cancellationToken);
			response.EnsureSuccessStatusCode();
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "DELETE {Url} failed", url);
			throw;
		}
	}

	/// <summary>
	/// PATCH Request
	/// </summary>
	public async Task<TResponse?> PatchAsync<TRequest, TResponse>(
		string serviceName,
		string path,
		TRequest data,
		CancellationToken cancellationToken = default)
	{
		var url = BuildUrl(path);
		_logger?.LogDebug("PATCH {Url}", url);

		try
		{
			var response = await _httpClient.PatchAsJsonAsync(url, data, _jsonOptions, cancellationToken);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "PATCH {Url} failed", url);
			throw;
		}
	}

	/// <summary>
	/// بناء URL الكامل من Base URL + Path
	/// </summary>
	private string BuildUrl(string path)
	{
		var cleanPath = path.StartsWith('/') ? path : $"/{path}";
		return $"{_baseUrl}{cleanPath}";
	}
}

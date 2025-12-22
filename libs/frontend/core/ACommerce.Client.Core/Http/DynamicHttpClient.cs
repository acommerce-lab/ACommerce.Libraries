using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ACommerce.ServiceRegistry.Client;
using Microsoft.Extensions.Logging;

namespace ACommerce.Client.Core.Http;

/// <summary>
/// HTTP Client مع Dynamic Service URLs
/// يستخدم Service Registry للحصول على URLs ديناميكياً
/// </summary>
public sealed class DynamicHttpClient : IApiClient
{
	private readonly HttpClient _httpClient;
	private readonly ServiceRegistryClient _registryClient;
	private readonly ILogger<DynamicHttpClient> _logger;
	private readonly JsonSerializerOptions _jsonOptions;

	public DynamicHttpClient(
		HttpClient httpClient,
		ServiceRegistryClient registryClient,
		ILogger<DynamicHttpClient> logger)
	{
		_httpClient = httpClient;
		_registryClient = registryClient;
		_logger = logger;

		_jsonOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			Converters = { new JsonStringEnumConverter() }
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
		var url = await BuildUrlAsync(serviceName, path, cancellationToken);
		_logger.LogDebug("GET {Url}", url);

		try
		{
			var response = await _httpClient.GetAsync(url, cancellationToken);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "GET {Url} failed", url);
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
		var url = await BuildUrlAsync(serviceName, path, cancellationToken);
		_logger.LogDebug("POST {Url}", url);

		try
		{
			var response = await _httpClient.PostAsJsonAsync(url, data, _jsonOptions, cancellationToken);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "POST {Url} failed", url);
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
		var url = await BuildUrlAsync(serviceName, path, cancellationToken);
		_logger.LogDebug("POST {Url}", url);

		try
		{
			var response = await _httpClient.PostAsJsonAsync(url, data, _jsonOptions, cancellationToken);
			response.EnsureSuccessStatusCode();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "POST {Url} failed", url);
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
		var url = await BuildUrlAsync(serviceName, path, cancellationToken);
		_logger.LogDebug("PUT {Url}", url);

		try
		{
			var response = await _httpClient.PutAsJsonAsync(url, data, _jsonOptions, cancellationToken);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "PUT {Url} failed", url);
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
		var url = await BuildUrlAsync(serviceName, path, cancellationToken);
		_logger.LogDebug("PUT {Url}", url);

		try
		{
			var response = await _httpClient.PutAsJsonAsync(url, data, _jsonOptions, cancellationToken);
			response.EnsureSuccessStatusCode();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "PUT {Url} failed", url);
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
		var url = await BuildUrlAsync(serviceName, path, cancellationToken);
		_logger.LogDebug("DELETE {Url}", url);

		try
		{
			var response = await _httpClient.DeleteAsync(url, cancellationToken);
			response.EnsureSuccessStatusCode();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "DELETE {Url} failed", url);
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
		var url = await BuildUrlAsync(serviceName, path, cancellationToken);
		_logger.LogDebug("PATCH {Url}", url);

		try
		{
			var response = await _httpClient.PatchAsJsonAsync(url, data, _jsonOptions, cancellationToken);
			response.EnsureSuccessStatusCode();

			return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "PATCH {Url} failed", url);
			throw;
		}
	}

	/// <summary>
	/// بناء URL الكامل من Service Name + Path
	/// </summary>
	private async Task<string> BuildUrlAsync(
		string serviceName,
		string path,
		CancellationToken cancellationToken)
	{
		var endpoint = await _registryClient.DiscoverAsync(serviceName, cancellationToken);

		if (endpoint == null)
		{
			throw new InvalidOperationException($"Service not found: {serviceName}");
		}

		var baseUrl = endpoint.BaseUrl.TrimEnd('/');
		var cleanPath = path.StartsWith('/') ? path : $"/{path}";

		return $"{baseUrl}{cleanPath}";
	}
}

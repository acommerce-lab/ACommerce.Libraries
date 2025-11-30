namespace ACommerce.Client.Core.Http;

/// <summary>
/// واجهة موحدة للتعامل مع HTTP APIs
/// تسمح بالتبديل بين DynamicHttpClient و StaticHttpClient
/// </summary>
public interface IApiClient
{
	/// <summary>
	/// GET Request
	/// </summary>
	Task<T?> GetAsync<T>(
		string serviceName,
		string path,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// POST Request مع Response
	/// </summary>
	Task<TResponse?> PostAsync<TRequest, TResponse>(
		string serviceName,
		string path,
		TRequest data,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// POST Request بدون Response
	/// </summary>
	Task PostAsync<TRequest>(
		string serviceName,
		string path,
		TRequest data,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// PUT Request مع Response
	/// </summary>
	Task<TResponse?> PutAsync<TRequest, TResponse>(
		string serviceName,
		string path,
		TRequest data,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// PUT Request بدون Response
	/// </summary>
	Task PutAsync<TRequest>(
		string serviceName,
		string path,
		TRequest data,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// DELETE Request
	/// </summary>
	Task DeleteAsync(
		string serviceName,
		string path,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// PATCH Request
	/// </summary>
	Task<TResponse?> PatchAsync<TRequest, TResponse>(
		string serviceName,
		string path,
		TRequest data,
		CancellationToken cancellationToken = default);
}

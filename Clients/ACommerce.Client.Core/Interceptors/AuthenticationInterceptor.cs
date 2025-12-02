using System.Net.Http.Headers;

namespace ACommerce.Client.Core.Interceptors;

/// <summary>
/// Interceptor لإضافة Authentication Token تلقائياً لكل طلب
/// </summary>
public sealed class AuthenticationInterceptor : DelegatingHandler
{
	private readonly ITokenProvider _tokenProvider;

	public AuthenticationInterceptor(ITokenProvider tokenProvider)
	{
		_tokenProvider = tokenProvider;
		Console.WriteLine($"[AuthInterceptor] Created with TokenProvider: {tokenProvider.GetType().Name} (HashCode: {tokenProvider.GetHashCode()})");
	}

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		// الحصول على Token
		var token = await _tokenProvider.GetTokenAsync();

		Console.WriteLine($"[AuthInterceptor] Request to {request.RequestUri?.AbsolutePath} - Token: {(string.IsNullOrEmpty(token) ? "NULL" : token.Substring(0, Math.Min(20, token.Length)) + "...")}");

		if (!string.IsNullOrEmpty(token))
		{
			// إضافة Authorization Header
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			Console.WriteLine($"[AuthInterceptor] ✅ Authorization header added");
		}
		else
		{
			Console.WriteLine($"[AuthInterceptor] ⚠️ No token available - request will be unauthorized");
		}

		return await base.SendAsync(request, cancellationToken);
	}
}

/// <summary>
/// واجهة للحصول على Authentication Token
/// </summary>
public interface ITokenProvider
{
	Task<string?> GetTokenAsync();
}

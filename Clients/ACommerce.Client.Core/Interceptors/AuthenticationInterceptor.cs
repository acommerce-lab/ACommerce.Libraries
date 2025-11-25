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
	}

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		// الحصول على Token
		var token = await _tokenProvider.GetTokenAsync();

		if (!string.IsNullOrEmpty(token))
		{
			// إضافة Authorization Header
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

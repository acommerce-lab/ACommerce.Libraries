using System.Globalization;

namespace ACommerce.Client.Core.Interceptors;

/// <summary>
/// Interceptor لإضافة اللغة الحالية في Headers تلقائياً
/// </summary>
public sealed class LocalizationInterceptor : DelegatingHandler
{
	private readonly ILocalizationProvider _localizationProvider;

	public LocalizationInterceptor(ILocalizationProvider localizationProvider)
	{
		_localizationProvider = localizationProvider;
	}

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken)
	{
		// الحصول على اللغة الحالية
		var currentLanguage = await _localizationProvider.GetCurrentLanguageAsync();

		if (!string.IsNullOrEmpty(currentLanguage))
		{
			// إضافة Headers
			request.Headers.Add("Accept-Language", currentLanguage);
			request.Headers.Add("X-Localization", currentLanguage);
		}

		// إضافة Culture Info
		var culture = await _localizationProvider.GetCurrentCultureAsync();
		if (!string.IsNullOrEmpty(culture))
		{
			request.Headers.Add("X-Culture", culture);
		}

		return await base.SendAsync(request, cancellationToken);
	}
}

/// <summary>
/// واجهة للحصول على معلومات Localization
/// </summary>
public interface ILocalizationProvider
{
	/// <summary>
	/// الحصول على اللغة الحالية (مثل: "ar", "en", "fr")
	/// </summary>
	Task<string> GetCurrentLanguageAsync();

	/// <summary>
	/// الحصول على Culture الحالية (مثل: "ar-SA", "en-US")
	/// </summary>
	Task<string> GetCurrentCultureAsync();

	/// <summary>
	/// تغيير اللغة
	/// </summary>
	Task SetLanguageAsync(string language);
}

/// <summary>
/// Default implementation يستخدم CurrentCulture
/// </summary>
public sealed class DefaultLocalizationProvider : ILocalizationProvider
{
	private string? _currentLanguage;

	public Task<string> GetCurrentLanguageAsync()
	{
		if (!string.IsNullOrEmpty(_currentLanguage))
			return Task.FromResult(_currentLanguage);

		// استخدام اللغة من النظام
		var culture = CultureInfo.CurrentUICulture;
		return Task.FromResult(culture.TwoLetterISOLanguageName);
	}

	public Task<string> GetCurrentCultureAsync()
	{
		var culture = CultureInfo.CurrentCulture;
		return Task.FromResult(culture.Name);
	}

	public Task SetLanguageAsync(string language)
	{
		_currentLanguage = language;

		// تغيير Culture
		var culture = new CultureInfo(language);
		CultureInfo.CurrentCulture = culture;
		CultureInfo.CurrentUICulture = culture;

		return Task.CompletedTask;
	}
}

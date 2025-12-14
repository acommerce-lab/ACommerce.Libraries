namespace ACommerce.Templates.Customer.Services;

/// <summary>
/// Localization service interface
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Current language code
    /// </summary>
    string CurrentLanguage { get; }

    /// <summary>
    /// Supported languages
    /// </summary>
    IReadOnlyList<LanguageInfo> SupportedLanguages { get; }

    /// <summary>
    /// Is right-to-left direction
    /// </summary>
    bool IsRtl { get; }

    /// <summary>
    /// Change language
    /// </summary>
    Task SetLanguageAsync(string languageCode);

    /// <summary>
    /// Get translated text
    /// </summary>
    string this[string key] { get; }

    /// <summary>
    /// Get translated text with format parameters
    /// </summary>
    string Get(string key, params object[] args);

    /// <summary>
    /// Language changed event
    /// </summary>
    event Action? OnLanguageChanged;
}

/// <summary>
/// Language information
/// </summary>
public class LanguageInfo
{
    public string Code { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;
    public bool IsRtl { get; set; }
}

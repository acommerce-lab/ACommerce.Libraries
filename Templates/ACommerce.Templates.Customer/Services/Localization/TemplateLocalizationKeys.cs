namespace ACommerce.Templates.Customer.Services.Localization;

/// <summary>
/// Required localization keys that tenant apps must provide
/// </summary>
public static class TemplateLocalizationKeys
{
    /// <summary>
    /// Brand/App specific keys - MUST be provided by tenant
    /// </summary>
    public static class Required
    {
        public const string AppName = "AppName";
        public const string AppTagline = "AppTagline";
    }

    /// <summary>
    /// Optional brand keys - can be overridden by tenant
    /// </summary>
    public static class Optional
    {
        public const string WelcomeMessage = "WelcomeMessage";
        public const string AboutApp = "AboutApp";
        public const string ContactEmail = "ContactEmail";
        public const string SupportPhone = "SupportPhone";
    }

    /// <summary>
    /// Get all required keys
    /// </summary>
    public static IReadOnlyList<string> GetRequiredKeys() =>
    [
        Required.AppName,
        Required.AppTagline
    ];

    /// <summary>
    /// Get all optional brand keys
    /// </summary>
    public static IReadOnlyList<string> GetOptionalBrandKeys() =>
    [
        Optional.WelcomeMessage,
        Optional.AboutApp,
        Optional.ContactEmail,
        Optional.SupportPhone
    ];
}

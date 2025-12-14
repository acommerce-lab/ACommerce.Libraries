using ACommerce.Templates.Customer.Services;
using Microsoft.Extensions.Logging;

namespace ACommerce.Templates.Customer.Services.Localization;

public class LocalizationValidationService
{
    private readonly ILocalizationService _localization;
    private readonly ILogger<LocalizationValidationService> _logger;

    public LocalizationValidationService(
        ILocalizationService localization,
        ILogger<LocalizationValidationService> logger)
    {
        _localization = localization;
        _logger = logger;
    }

    public void ValidateOnStartup(bool isDevelopment)
    {
        if (!isDevelopment)
            return;

        _logger.LogInformation("🌍 Validating localization keys...");

        if (_localization is BaseLocalizationService baseService)
        {
            var missingKeys = baseService.ValidateRequiredKeys();
            
            if (missingKeys.Count > 0)
            {
                _logger.LogWarning("⚠️ ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                _logger.LogWarning("⚠️ MISSING REQUIRED TRANSLATIONS ({Count} keys):", missingKeys.Count);
                _logger.LogWarning("⚠️ ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                
                foreach (var key in missingKeys)
                {
                    _logger.LogWarning("   ❌ {Key}", key);
                }
                
                _logger.LogWarning("⚠️ ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                _logger.LogWarning("⚠️ Add these keys to your LocalizationService.ConfigureTranslations()");
                _logger.LogWarning("⚠️ Required keys: AppName, AppTagline");
                _logger.LogWarning("⚠️ ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
            else
            {
                _logger.LogInformation("✅ All required localization keys are present");
            }
        }
    }
}

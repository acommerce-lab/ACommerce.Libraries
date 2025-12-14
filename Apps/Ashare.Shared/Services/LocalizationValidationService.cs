using ACommerce.Templates.Customer.Services.Localization;

namespace Ashare.Shared.Services;

/// <summary>
/// خدمة للتحقق من وجود جميع الترجمات المطلوبة
/// </summary>
public class LocalizationValidationService
{
    private readonly LocalizationService _localizationService;

    public LocalizationValidationService(LocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    /// <summary>
    /// التحقق من وجود جميع المفاتيح المطلوبة
    /// </summary>
    public LocalizationValidationResult Validate()
    {
        var missingKeys = _localizationService.ValidateRequiredKeys();
        
        return new LocalizationValidationResult
        {
            IsValid = missingKeys.Count == 0,
            MissingKeys = missingKeys.ToList()
        };
    }

    /// <summary>
    /// التحقق والإبلاغ عن أي مفاتيح مفقودة في الـ console
    /// </summary>
    public void ValidateAndReport()
    {
        var result = Validate();
        
        if (!result.IsValid)
        {
            Console.WriteLine("⚠️ Missing localization keys:");
            foreach (var key in result.MissingKeys)
            {
                Console.WriteLine($"  - {key}");
            }
        }
    }
}

public class LocalizationValidationResult
{
    public bool IsValid { get; set; }
    public List<string> MissingKeys { get; set; } = new();
}

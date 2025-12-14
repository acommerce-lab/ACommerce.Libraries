using System.Globalization;

namespace ACommerce.Templates.Customer.Services.Localization;

/// <summary>
/// Base localization service with common translations
/// Tenant apps should inherit and add/override translations
/// </summary>
public abstract class BaseLocalizationService : ILocalizationService
{
    private string _currentLanguage = "en";
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    private readonly IStorageService _storageService;
    private readonly List<LanguageInfo> _supportedLanguages;

    protected BaseLocalizationService(IStorageService storageService)
    {
        _storageService = storageService;
        _supportedLanguages = new List<LanguageInfo>();
        _translations = new Dictionary<string, Dictionary<string, string>>();

        // Configure languages
        ConfigureLanguages(_supportedLanguages);

        // Initialize with base translations
        foreach (var lang in _supportedLanguages)
        {
            _translations[lang.Code] = new Dictionary<string, string>();
        }

        // Add base translations
        AddBaseTranslations();

        // Allow tenant to add/override translations
        ConfigureTranslations(_translations);

        // Set default language
        _currentLanguage = _supportedLanguages.FirstOrDefault()?.Code ?? "en";

        // Load saved language
        _ = LoadSavedLanguageAsync();
    }

    public string CurrentLanguage => _currentLanguage;

    public IReadOnlyList<LanguageInfo> SupportedLanguages => _supportedLanguages;

    public bool IsRtl => _supportedLanguages.FirstOrDefault(l => l.Code == _currentLanguage)?.IsRtl ?? false;

    public event Action? OnLanguageChanged;

    public string this[string key] => Get(key);

    public string Get(string key, params object[] args)
    {
        // Try current language
        if (_translations.TryGetValue(_currentLanguage, out var langStrings) &&
            langStrings.TryGetValue(key, out var value))
        {
            return args.Length > 0 ? string.Format(value, args) : value;
        }

        // Fallback to English
        if (_translations.TryGetValue("en", out var enStrings) &&
            enStrings.TryGetValue(key, out var enValue))
        {
            return args.Length > 0 ? string.Format(enValue, args) : enValue;
        }

        // Return key if not found
        return key;
    }

    /// <summary>
    /// Check if a translation key exists
    /// </summary>
    public bool HasKey(string key)
    {
        return _translations.TryGetValue(_currentLanguage, out var langStrings) &&
               langStrings.ContainsKey(key);
    }

    /// <summary>
    /// Get translation or null if not found
    /// </summary>
    public string? GetOrNull(string key)
    {
        if (_translations.TryGetValue(_currentLanguage, out var langStrings) &&
            langStrings.TryGetValue(key, out var value))
        {
            return value;
        }
        return null;
    }

    public async Task SetLanguageAsync(string languageCode)
    {
        if (_currentLanguage == languageCode) return;
        if (!_supportedLanguages.Any(l => l.Code == languageCode)) return;

        _currentLanguage = languageCode;
        CultureInfo.CurrentCulture = new CultureInfo(languageCode);
        CultureInfo.CurrentUICulture = new CultureInfo(languageCode);

        try
        {
            await _storageService.SetAsync("app_language", languageCode);
        }
        catch { }

        OnLanguageChanged?.Invoke();
    }

    private async Task LoadSavedLanguageAsync()
    {
        try
        {
            var saved = await _storageService.GetAsync("app_language");
            if (!string.IsNullOrEmpty(saved) &&
                _supportedLanguages.Any(l => l.Code == saved) &&
                saved != _currentLanguage)
            {
                _currentLanguage = saved;
                CultureInfo.CurrentCulture = new CultureInfo(saved);
                CultureInfo.CurrentUICulture = new CultureInfo(saved);
                OnLanguageChanged?.Invoke();
            }
        }
        catch { }
    }

    /// <summary>
    /// Validate that all required keys are present
    /// </summary>
    public IReadOnlyList<string> ValidateRequiredKeys()
    {
        var missingKeys = new List<string>();

        foreach (var key in TemplateLocalizationKeys.GetRequiredKeys())
        {
            foreach (var lang in _supportedLanguages)
            {
                if (!_translations.TryGetValue(lang.Code, out var langStrings) ||
                    !langStrings.ContainsKey(key))
                {
                    missingKeys.Add($"{key} ({lang.Code})");
                }
            }
        }

        return missingKeys;
    }

    /// <summary>
    /// Override to configure supported languages
    /// </summary>
    protected virtual void ConfigureLanguages(List<LanguageInfo> languages)
    {
        languages.Add(new LanguageInfo { Code = "en", NativeName = "English", EnglishName = "English", IsRtl = false });
        languages.Add(new LanguageInfo { Code = "ar", NativeName = "العربية", EnglishName = "Arabic", IsRtl = true });
    }

    /// <summary>
    /// Override to add/override translations
    /// </summary>
    protected abstract void ConfigureTranslations(Dictionary<string, Dictionary<string, string>> translations);

    /// <summary>
    /// Add base translations (common UI elements)
    /// </summary>
    private void AddBaseTranslations()
    {
        // English base translations
        if (_translations.ContainsKey("en"))
        {
            var en = _translations["en"];

            // Navigation
            en["Home"] = "Home";
            en["Explore"] = "Explore";
            en["Search"] = "Search";
            en["Favorites"] = "Favorites";
            en["Profile"] = "Profile";
            en["Settings"] = "Settings";
            en["Notifications"] = "Notifications";
            en["Messages"] = "Messages";

            // Auth
            en["Login"] = "Login";
            en["Logout"] = "Logout";
            en["Register"] = "Register";
            en["CreateAccount"] = "Create Account";
            en["WelcomeBack"] = "Welcome Back";
            en["Email"] = "Email";
            en["Password"] = "Password";
            en["ConfirmPassword"] = "Confirm Password";
            en["FullName"] = "Full Name";
            en["Phone"] = "Phone";
            en["ForgotPassword"] = "Forgot Password?";
            en["RememberMe"] = "Remember Me";
            en["DontHaveAccount"] = "Don't have an account?";
            en["AlreadyHaveAccount"] = "Already have an account?";
            en["TermsAndConditions"] = "Terms & Conditions";
            en["PrivacyPolicy"] = "Privacy Policy";
            en["IAccept"] = "I accept";
            en["And"] = "and";
            en["Or"] = "or";
            en["ContinueAsGuest"] = "Continue as Guest";

            // Common Actions
            en["Save"] = "Save";
            en["Cancel"] = "Cancel";
            en["Delete"] = "Delete";
            en["Edit"] = "Edit";
            en["Back"] = "Back";
            en["Next"] = "Next";
            en["Done"] = "Done";
            en["Close"] = "Close";
            en["Yes"] = "Yes";
            en["No"] = "No";
            en["OK"] = "OK";
            en["Apply"] = "Apply";
            en["Reset"] = "Reset";
            en["Retry"] = "Retry";
            en["Continue"] = "Continue";
            en["Submit"] = "Submit";
            en["Confirm"] = "Confirm";
            en["ViewAll"] = "View All";
            en["TryAgain"] = "Try Again";

            // Status
            en["Loading"] = "Loading...";
            en["Error"] = "Error";
            en["Success"] = "Success";
            en["Pending"] = "Pending";
            en["Active"] = "Active";
            en["Inactive"] = "Inactive";
            en["Completed"] = "Completed";
            en["Cancelled"] = "Cancelled";
            en["Processing"] = "Processing...";

            // Time
            en["Now"] = "Now";
            en["Today"] = "Today";
            en["Yesterday"] = "Yesterday";
            en["Tomorrow"] = "Tomorrow";
            en["Date"] = "Date";
            en["Time"] = "Time";
            en["Hour"] = "hour";
            en["Day"] = "day";
            en["Week"] = "week";
            en["Month"] = "month";
            en["Year"] = "year";

            // Misc
            en["Optional"] = "Optional";
            en["Required"] = "Required";
            en["Description"] = "Description";
            en["Details"] = "Details";
            en["Total"] = "Total";
            en["Subtotal"] = "Subtotal";
            en["Featured"] = "Featured";
            en["New"] = "New";
            en["Popular"] = "Popular";
            en["Recommended"] = "Recommended";
            en["Categories"] = "Categories";
            en["Filters"] = "Filters";
            en["SortBy"] = "Sort By";
            en["Results"] = "Results";
            en["NoResults"] = "No Results";

            // Profile
            en["EditProfile"] = "Edit Profile";
            en["PersonalInfo"] = "Personal Info";
            en["ContactInfo"] = "Contact Info";
            en["ChangePhoto"] = "Change Photo";
            en["ChangePassword"] = "Change Password";
            en["SaveChanges"] = "Save Changes";
            en["Language"] = "Language";
            en["DarkMode"] = "Dark Mode";
            en["Help"] = "Help";
            en["About"] = "About";
            en["Version"] = "Version";

            // Errors
            en["SomethingWentWrong"] = "Something went wrong";
            en["NetworkError"] = "Network error. Please check your connection.";
            en["SessionExpired"] = "Session expired. Please login again.";
            en["Unauthorized"] = "Unauthorized access";
            en["NotFound"] = "Not found";
        }

        // Arabic base translations
        if (_translations.ContainsKey("ar"))
        {
            var ar = _translations["ar"];

            // Navigation
            ar["Home"] = "الرئيسية";
            ar["Explore"] = "استكشف";
            ar["Search"] = "البحث";
            ar["Favorites"] = "المفضلة";
            ar["Profile"] = "حسابي";
            ar["Settings"] = "الإعدادات";
            ar["Notifications"] = "الإشعارات";
            ar["Messages"] = "الرسائل";

            // Auth
            ar["Login"] = "تسجيل الدخول";
            ar["Logout"] = "تسجيل الخروج";
            ar["Register"] = "إنشاء حساب";
            ar["CreateAccount"] = "إنشاء حساب جديد";
            ar["WelcomeBack"] = "مرحباً بعودتك";
            ar["Email"] = "البريد الإلكتروني";
            ar["Password"] = "كلمة المرور";
            ar["ConfirmPassword"] = "تأكيد كلمة المرور";
            ar["FullName"] = "الاسم الكامل";
            ar["Phone"] = "رقم الجوال";
            ar["ForgotPassword"] = "نسيت كلمة المرور؟";
            ar["RememberMe"] = "تذكرني";
            ar["DontHaveAccount"] = "ليس لديك حساب؟";
            ar["AlreadyHaveAccount"] = "لديك حساب بالفعل؟";
            ar["TermsAndConditions"] = "الشروط والأحكام";
            ar["PrivacyPolicy"] = "سياسة الخصوصية";
            ar["IAccept"] = "أوافق على";
            ar["And"] = "و";
            ar["Or"] = "أو";
            ar["ContinueAsGuest"] = "المتابعة كضيف";

            // Common Actions
            ar["Save"] = "حفظ";
            ar["Cancel"] = "إلغاء";
            ar["Delete"] = "حذف";
            ar["Edit"] = "تعديل";
            ar["Back"] = "رجوع";
            ar["Next"] = "التالي";
            ar["Done"] = "تم";
            ar["Close"] = "إغلاق";
            ar["Yes"] = "نعم";
            ar["No"] = "لا";
            ar["OK"] = "موافق";
            ar["Apply"] = "تطبيق";
            ar["Reset"] = "إعادة تعيين";
            ar["Retry"] = "إعادة المحاولة";
            ar["Continue"] = "متابعة";
            ar["Submit"] = "إرسال";
            ar["Confirm"] = "تأكيد";
            ar["ViewAll"] = "عرض الكل";
            ar["TryAgain"] = "حاول مرة أخرى";

            // Status
            ar["Loading"] = "جاري التحميل...";
            ar["Error"] = "خطأ";
            ar["Success"] = "نجاح";
            ar["Pending"] = "قيد الانتظار";
            ar["Active"] = "نشط";
            ar["Inactive"] = "غير نشط";
            ar["Completed"] = "مكتمل";
            ar["Cancelled"] = "ملغي";
            ar["Processing"] = "جاري المعالجة...";

            // Time
            ar["Now"] = "الآن";
            ar["Today"] = "اليوم";
            ar["Yesterday"] = "أمس";
            ar["Tomorrow"] = "غداً";
            ar["Date"] = "التاريخ";
            ar["Time"] = "الوقت";
            ar["Hour"] = "ساعة";
            ar["Day"] = "يوم";
            ar["Week"] = "أسبوع";
            ar["Month"] = "شهر";
            ar["Year"] = "سنة";

            // Misc
            ar["Optional"] = "اختياري";
            ar["Required"] = "مطلوب";
            ar["Description"] = "الوصف";
            ar["Details"] = "التفاصيل";
            ar["Total"] = "المجموع";
            ar["Subtotal"] = "المجموع الفرعي";
            ar["Featured"] = "مميز";
            ar["New"] = "جديد";
            ar["Popular"] = "شائع";
            ar["Recommended"] = "موصى به";
            ar["Categories"] = "الفئات";
            ar["Filters"] = "تصفية";
            ar["SortBy"] = "ترتيب حسب";
            ar["Results"] = "نتائج";
            ar["NoResults"] = "لا توجد نتائج";

            // Profile
            ar["EditProfile"] = "تعديل الملف الشخصي";
            ar["PersonalInfo"] = "البيانات الشخصية";
            ar["ContactInfo"] = "بيانات التواصل";
            ar["ChangePhoto"] = "تغيير الصورة";
            ar["ChangePassword"] = "تغيير كلمة المرور";
            ar["SaveChanges"] = "حفظ التغييرات";
            ar["Language"] = "اللغة";
            ar["DarkMode"] = "الوضع الداكن";
            ar["Help"] = "المساعدة";
            ar["About"] = "حول";
            ar["Version"] = "الإصدار";

            // Errors
            ar["SomethingWentWrong"] = "حدث خطأ ما";
            ar["NetworkError"] = "خطأ في الاتصال. يرجى التحقق من اتصالك.";
            ar["SessionExpired"] = "انتهت صلاحية الجلسة. يرجى تسجيل الدخول مرة أخرى.";
            ar["Unauthorized"] = "غير مصرح بالوصول";
            ar["NotFound"] = "غير موجود";
        }
    }
}
